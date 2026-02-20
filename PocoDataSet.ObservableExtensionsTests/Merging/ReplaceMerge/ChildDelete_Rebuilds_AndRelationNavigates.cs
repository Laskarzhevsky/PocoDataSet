using System;
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
    /// Ensures Replace merge keeps relation navigation consistent and raises collection notifications that match
    /// Replace's "rebuild rows" behavior for the child table.
    ///
    /// Replace rebuilds the Orders row collection, so we assert exact Add/Remove counts:
    /// - Remove == original Orders row count
    /// - Add    == refreshed Orders row count
    /// </summary>

    [Fact]
    public void ChildDelete_Rebuilds_AndRelationNavigates()
    {
        // Arrange CURRENT with relation metadata.
        IDataSet currentInner = DataSetFactory.CreateDataSet();
        IObservableDataSet current = new ObservableDataSet(currentInner);

        IObservableDataTable customers = current.AddNewTable("Customers");
        customers.AddColumn("Id", DataTypeNames.INT32, false, true);
        customers.AddColumn("Name", DataTypeNames.STRING);

        IObservableDataTable orders = current.AddNewTable("Orders");
        orders.AddColumn("OrderId", DataTypeNames.INT32, false, true);
        orders.AddColumn("CustomerId", DataTypeNames.INT32, false, false);
        orders.AddColumn("Status", DataTypeNames.STRING);

        current.InnerDataSet.AddRelation(
            "CustomerOrders",
            "Customers",
            new List<string> { "Id" },
            "Orders",
            new List<string> { "CustomerId" });

        IObservableDataRow c1 = customers.AddNewRow();
        c1["Id"] = 1;
        c1["Name"] = "Acme";
        c1.AcceptChanges();

IObservableDataRow o10 = orders.AddNewRow();
o10["OrderId"] = 10;
o10["CustomerId"] = 1;
o10["Status"] = "Open";
o10.AcceptChanges();

IObservableDataRow o11 = orders.AddNewRow();
o11["OrderId"] = 11;
o11["CustomerId"] = 1;
o11["Status"] = "Open";
o11.AcceptChanges();

CollectionChangedCounter counter = new CollectionChangedCounter();
orders.CollectionChanged += counter.Handler;

// Arrange REFRESHED snapshot.
IDataSet refreshed = DataSetFactory.CreateDataSet();

IDataTable rCustomers = refreshed.AddNewTable("Customers");
rCustomers.AddColumn("Id", DataTypeNames.INT32, false, true);
rCustomers.AddColumn("Name", DataTypeNames.STRING);

IDataTable rOrders = refreshed.AddNewTable("Orders");
rOrders.AddColumn("OrderId", DataTypeNames.INT32, false, true);
rOrders.AddColumn("CustomerId", DataTypeNames.INT32, false, false);
rOrders.AddColumn("Status", DataTypeNames.STRING);

IDataRow rc1 = rCustomers.AddNewRow();
rc1["Id"] = 1;
rc1["Name"] = "Acme";

IDataRow ro10 = rOrders.AddNewRow();
ro10["OrderId"] = 10;
ro10["CustomerId"] = 1;
ro10["Status"] = "Open";

IObservableMergeOptions options = new ObservableMergeOptions();

// Act
current.DoReplaceMerge(refreshed, options);

// Assert: Replace rebuilds Orders rows.
Assert.Equal(1, counter.AddEvents);
Assert.Equal(2, counter.RemoveEvents);

// Assert: relation navigation matches refreshed snapshot immediately after merge.
IDataRow mergedCustomer = current.InnerDataSet.Tables["Customers"].Rows[0];
List<IDataRow> children = current.InnerDataSet.GetChildRows("CustomerOrders", mergedCustomer);
Assert.Equal(1, children.Count);

Assert.Equal(10, (int)children[0]["OrderId"]!);

        Assert.Empty(current.InnerDataSet.ValidateRelations());
    }
}
}
