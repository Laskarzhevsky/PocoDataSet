
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class ReplaceMerge
    {
        /// <summary>
        /// Locks relational integrity invariants for the ReplaceMerge merge mode (observable wrapper).
        ///
        /// Why this test exists:
        /// - Observable merge operates on observable wrappers, but relations are stored on the underlying IDataSet
        ///   (ObservableDataSet.InnerDataSet.Relations).
        /// - Because observable components are layered on top of non-observable components, we expect the same
        ///   "relations survive merge" contract to hold in observable merge modes as well.
        ///
        /// Scenario:
        /// - CURRENT (inner data set) has Customers (parent) and Orders (child).
        /// - A relation "CustomerOrders" connects Customers.Id -> Orders.CustomerId.
        /// - CURRENT contains one customer (Id=1) and two orders referencing that customer.
        /// - REFRESHED changes some data but does NOT define relations.
        ///
        /// How the test proves the contract:
        /// - Assert #1: relation metadata still exists after the merge (count + mapping).
        /// - Assert #2: child navigation works after merge: GetChildRows returns both orders for the customer.
        /// - Assert #3: parent navigation works after merge: TryGetParentRow finds the customer for an order.
        /// - Assert #4: ValidateRelations reports no violations after the merge.
        /// </summary>
        [Fact]
        public void Relations_Preserved_Navigation_Works()
        {
            // Arrange CURRENT (inner data set with relation metadata).
            IDataSet currentInner = DataSetFactory.CreateDataSet();

            IDataTable customers = currentInner.AddNewTable("Customers");
            customers.AddColumn("Id", DataTypeNames.INT32, false, true);
            customers.AddColumn("Name", DataTypeNames.STRING);

            IDataTable orders = currentInner.AddNewTable("Orders");
            orders.AddColumn("OrderId", DataTypeNames.INT32, false, true);
            orders.AddColumn("CustomerId", DataTypeNames.INT32, false, false);
            orders.AddColumn("Status", DataTypeNames.STRING);

            currentInner.AddRelation(
                "CustomerOrders",
                "Customers",
                new List<string> { "Id" },
                "Orders",
                new List<string> { "CustomerId" });

            // Parent row.
            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(customers.Columns);
            c1["Id"] = 1;
            c1["Name"] = "Acme";
            customers.AddLoadedRow(c1);

            // Child rows.
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

            customers.AcceptChanges();
            orders.AcceptChanges();

            // Sanity: relations validate before merge.
            Assert.Empty(currentInner.ValidateRelations());

            // Wrap CURRENT in observable.
            IObservableDataSet current = new ObservableDataSet(currentInner);

            // Arrange REFRESHED without relations.
            IDataSet refreshed = DataSetFactory.CreateDataSet();

            IDataTable rCustomers = refreshed.AddNewTable("Customers");
            rCustomers.AddColumn("Id", DataTypeNames.INT32, false, true);
            rCustomers.AddColumn("Name", DataTypeNames.STRING);

            IDataTable rOrders = refreshed.AddNewTable("Orders");
            rOrders.AddColumn("OrderId", DataTypeNames.INT32, false, true);
            rOrders.AddColumn("CustomerId", DataTypeNames.INT32, false, false);
            rOrders.AddColumn("Status", DataTypeNames.STRING);

            IDataRow rc1 = DataRowExtensions.CreateRowFromColumns(rCustomers.Columns);
            rc1["Id"] = 1;
            rc1["Name"] = "Acme (refreshed)";
            rCustomers.AddLoadedRow(rc1);

            IDataRow ro10 = DataRowExtensions.CreateRowFromColumns(rOrders.Columns);
            ro10["OrderId"] = 10;
            ro10["CustomerId"] = 1;
            ro10["Status"] = "Open";
            rOrders.AddLoadedRow(ro10);

            // Make one server row a true "Modified" row where relevant.
            ro10.AcceptChanges();
            ro10["Status"] = "Closed";

            IDataRow ro11 = DataRowExtensions.CreateRowFromColumns(rOrders.Columns);
            ro11["OrderId"] = 11;
            ro11["CustomerId"] = 1;
            ro11["Status"] = "Open";
            rOrders.AddLoadedRow(ro11);

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert #1: relation metadata still exists after merge.
            Assert.NotNull(current.InnerDataSet.Relations);
            Assert.Single(current.InnerDataSet.Relations);
            Assert.Equal("CustomerOrders", current.InnerDataSet.Relations[0].RelationName);
            Assert.Equal("Customers", current.InnerDataSet.Relations[0].ParentTableName);
            Assert.Equal("Orders", current.InnerDataSet.Relations[0].ChildTableName);
            Assert.Equal("Id", current.InnerDataSet.Relations[0].ParentColumnNames[0]);
            Assert.Equal("CustomerId", current.InnerDataSet.Relations[0].ChildColumnNames[0]);

            // Get parent row AFTER merge.
            IDataRow mergedCustomer = current.InnerDataSet.Tables["Customers"].Rows[0];

            // Assert #2: child navigation still returns both child rows for customer #1.
            List<IDataRow> mergedChildren = current.InnerDataSet.GetChildRows("CustomerOrders", mergedCustomer);
            Assert.Equal(2, mergedChildren.Count);

            // Assert #3: parent navigation works for a child row.
            IDataRow anyChild = mergedChildren[0];
            bool found = current.InnerDataSet.TryGetParentRow("CustomerOrders", anyChild, true, out IDataRow? parentRow);
            Assert.True(found);
            Assert.NotNull(parentRow);
            Assert.Equal(1, parentRow!["Id"]);

            // Assert #4: merged data set remains referentially valid.
            Assert.Empty(current.InnerDataSet.ValidateRelations());
        }
    }
}
