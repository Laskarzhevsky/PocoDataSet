using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks relational integrity invariants for the PostSaveMerge merge mode.
    ///
    /// Why this test exists:
    /// - Merge tests typically focus on rows, schema and merge-result accounting.
    /// - Relations are stored as metadata on the DataSet (IDataSet.Relations) and navigation is performed
    ///   by extension methods (GetChildRows / TryGetParentRow) that rely on that metadata and row values.
    /// - A merge must not "forget" relations or leave the data in a state where parent/child navigation breaks.
    ///
    /// Scenario:
    /// - CURRENT has two tables: Customers (parent) and Orders (child).
    /// - A relation "CustomerOrders" connects Customers.Id -> Orders.CustomerId.
    /// - CURRENT contains one customer (Id=1) and two orders referencing that customer.
    /// - REFRESHED/CHANGESET changes some data but does NOT define relations.
    ///
    /// How the test proves the contract:
    /// - Assert #1: The relation metadata still exists after the merge (count + name + table/column mapping).
    /// - Assert #2: Child navigation works: GetChildRows returns the correct orders for the customer.
    /// - Assert #3: Parent navigation works: TryGetParentRow finds the correct customer for an order.
    /// - Assert #4: Referential integrity validation reports no violations.
    ///
    /// Notes:
    /// - This test intentionally does NOT add relations to the refreshed data set. The contract is that merge
    ///   keeps CURRENT schema/metadata (including relations) and merges data only.
    /// - This file contains exactly one test method: Relations_Preserved_Navigation_Works.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void Relations_Preserved_Navigation_Works()
        {
            // Arrange: Build CURRENT.
            IDataSet current = DataSetFactory.CreateDataSet();

            IDataTable customers = current.AddNewTable("Customers");
            customers.AddColumn("Id", DataTypeNames.INT32, false, true);
            customers.AddColumn("Name", DataTypeNames.STRING);

            IDataTable orders = current.AddNewTable("Orders");
            orders.AddColumn("OrderId", DataTypeNames.INT32, false, true);
            orders.AddColumn("CustomerId", DataTypeNames.INT32, false, false);
            orders.AddColumn("Status", DataTypeNames.STRING);

            // Add relation metadata: Customers.Id -> Orders.CustomerId
            current.AddRelation(
                "CustomerOrders",
                "Customers",
                new List<string> { "Id" },
                "Orders",
                new List<string> { "CustomerId" });

            // Parent row
            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(customers.Columns);
            c1["Id"] = 1;
            c1["Name"] = "Acme";
            customers.AddLoadedRow(c1);

            // Child rows
            IDataRow o10 = DataRowExtensions.CreateRowFromColumns(orders.Columns);
            o10["OrderId"] = 10;
            o10["CustomerId"] = 1;
            o10["Status"] = "Open";
            orders.AddLoadedRow(o10);

            IDataRow o11 = DataRowExtensions.CreateRowFromColumns(orders.Columns);
            o11["OrderId"] = 11;
            o11["CustomerId"] = 1;
            o11["Status"] = "Open";
            orders.AddLoadedRow(o11);

            // Sanity: current relations validate before the merge.
            Assert.Empty(current.ValidateRelations());


            // For PostSave, CURRENT represents the client-side state after SaveChanges call but before applying
            // the server's post-save response. Ensure CURRENT has no pending changes to isolate relation behavior.
            customers.AcceptChanges();
            orders.AcceptChanges();

            // Arrange: Build REFRESHED/CHANGESET without any relation metadata.
            IDataSet refreshed = DataSetFactory.CreateDataSet();

            IDataTable rCustomers = refreshed.AddNewTable("Customers");
            rCustomers.AddColumn("Id", DataTypeNames.INT32, false, true);
            rCustomers.AddColumn("Name", DataTypeNames.STRING);

            IDataTable rOrders = refreshed.AddNewTable("Orders");
            rOrders.AddColumn("OrderId", DataTypeNames.INT32, false, true);
            rOrders.AddColumn("CustomerId", DataTypeNames.INT32, false, false);
            rOrders.AddColumn("Status", DataTypeNames.STRING);

            // Refresh customer name and modify one order, keep the other unchanged.
            IDataRow rc1 = DataRowExtensions.CreateRowFromColumns(rCustomers.Columns);
            rc1["Id"] = 1;
            rc1["Name"] = "Acme (refreshed)";
            rCustomers.AddLoadedRow(rc1);

            IDataRow ro10 = DataRowExtensions.CreateRowFromColumns(rOrders.Columns);
            ro10["OrderId"] = 10;
            ro10["CustomerId"] = 1;
            ro10["Status"] = "Open";
            rOrders.AddLoadedRow(ro10);

            // Make the server row a true "Modified" changeset row: AcceptChanges then change a value.
            ro10.AcceptChanges();
            ro10["Status"] = "Closed";

            IDataRow ro11 = DataRowExtensions.CreateRowFromColumns(rOrders.Columns);
            ro11["OrderId"] = 11;
            ro11["CustomerId"] = 1;
            ro11["Status"] = "Open";
            rOrders.AddLoadedRow(ro11);

            // Options / result container.
            MergeOptions options = new MergeOptions();

            // Act: run the merge.
            current.DoPostSaveMerge(refreshed, options);

            // Assert #1: relation metadata still exists after the merge.
            Assert.NotNull(current.Relations);
            Assert.Single(current.Relations);
            Assert.Equal("CustomerOrders", current.Relations[0].RelationName);
            Assert.Equal("Customers", current.Relations[0].ParentTableName);
            Assert.Equal("Orders", current.Relations[0].ChildTableName);
            Assert.Single(current.Relations[0].ParentColumnNames);
            Assert.Single(current.Relations[0].ChildColumnNames);
            Assert.Equal("Id", current.Relations[0].ParentColumnNames[0]);
            Assert.Equal("CustomerId", current.Relations[0].ChildColumnNames[0]);

            // Get the parent row instance AFTER merge (Replace rebuilds rows; other modes may not).
            IDataRow mergedCustomer = current.Tables["Customers"].Rows[0];

            // Assert #2: child navigation still returns both child rows for customer #1.
            List<IDataRow> mergedChildren = current.GetChildRows("CustomerOrders", mergedCustomer);
            Assert.Equal(2, mergedChildren.Count);

            // Assert #3: parent navigation works for a child row.
            IDataRow anyChild = mergedChildren[0];
            bool found = current.TryGetParentRow("CustomerOrders", anyChild, true, out IDataRow? parentRow);
            Assert.True(found);
            Assert.NotNull(parentRow);
            Assert.Equal(1, parentRow["Id"]);

            // Assert #4: the merged data set remains referentially valid.
            Assert.Empty(current.ValidateRelations());

        }
    }
}
