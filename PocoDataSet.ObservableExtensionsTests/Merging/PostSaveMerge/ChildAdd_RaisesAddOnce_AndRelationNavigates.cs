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

public partial class PostSaveMerge
{
    /// <summary>
    /// Ensures observable PostSave merge keeps relations consistent AND raises the expected collection notifications
    /// when child rows are added or finalized-removed.
    ///
    /// Notes about PostSave:
    /// - PostSave requires __ClientKey in CURRENT table schema.
    /// - Deletions are finalized when a CURRENT row is already marked Deleted and the server post-save snapshot
    ///   does not contain it.
    /// </summary>

    [Fact]
    public void ChildAdd_RaisesAddOnce_AndRelationNavigates()
    {
        // Arrange CURRENT with relation metadata and required __ClientKey columns.
        IDataSet currentInner = DataSetFactory.CreateDataSet();
        IObservableDataSet current = new ObservableDataSet(currentInner);

        IObservableDataTable customers = current.AddNewTable("Customers");
        customers.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
        customers.AddColumn("Id", DataTypeNames.INT32, false, true);
        customers.AddColumn("Name", DataTypeNames.STRING);

        IObservableDataTable orders = current.AddNewTable("Orders");
        orders.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
        orders.AddColumn("OrderId", DataTypeNames.INT32, false, true);
        orders.AddColumn("CustomerId", DataTypeNames.INT32, false, false);
        orders.AddColumn("Status", DataTypeNames.STRING);

        current.InnerDataSet.AddRelation(
            "CustomerOrders",
            "Customers",
            new List<string> { "Id" },
            "Orders",
            new List<string> { "CustomerId" });

        Guid customerKey = Guid.NewGuid();
        Guid order10Key = Guid.NewGuid();
        Guid order11Key = Guid.NewGuid();

        IObservableDataRow c1 = customers.AddNewRow();
        c1[SpecialColumnNames.CLIENT_KEY] = customerKey;
        c1["Id"] = 1;
        c1["Name"] = "Acme";
        c1.InnerDataRow.AcceptChanges();

IObservableDataRow o10 = orders.AddNewRow();
o10[SpecialColumnNames.CLIENT_KEY] = order10Key;
o10["OrderId"] = 10;
o10["CustomerId"] = 1;
o10["Status"] = "Open";
o10.InnerDataRow.AcceptChanges();

CollectionChangedCounter counter = new CollectionChangedCounter();
orders.CollectionChanged += counter.Handler;

// Arrange REFRESHED post-save snapshot.
IDataSet refreshed = DataSetFactory.CreateDataSet();

IDataTable rCustomers = refreshed.AddNewTable("Customers");
rCustomers.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
rCustomers.AddColumn("Id", DataTypeNames.INT32, false, true);
rCustomers.AddColumn("Name", DataTypeNames.STRING);

IDataTable rOrders = refreshed.AddNewTable("Orders");
rOrders.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
rOrders.AddColumn("OrderId", DataTypeNames.INT32, false, true);
rOrders.AddColumn("CustomerId", DataTypeNames.INT32, false, false);
rOrders.AddColumn("Status", DataTypeNames.STRING);

IDataRow rc1 = rCustomers.AddNewRow();
rc1[SpecialColumnNames.CLIENT_KEY] = customerKey;
rc1["Id"] = 1;
rc1["Name"] = "Acme";

IDataRow ro10 = rOrders.AddNewRow();
ro10[SpecialColumnNames.CLIENT_KEY] = order10Key;
ro10["OrderId"] = 10;
ro10["CustomerId"] = 1;
ro10["Status"] = "Open";

IDataRow ro11 = rOrders.AddNewRow();
ro11[SpecialColumnNames.CLIENT_KEY] = order11Key;
ro11["OrderId"] = 11;
ro11["CustomerId"] = 1;
ro11["Status"] = "Open";

IObservableMergeOptions options = new ObservableMergeOptions();

// Act
current.DoPostSaveMerge(refreshed, options);

// Assert: notification contract.
Assert.Equal(1, counter.AddEvents);
Assert.Equal(0, counter.RemoveEvents);

// Assert: relation navigation is consistent immediately after merge.
IDataRow mergedCustomer = current.InnerDataSet.Tables["Customers"].Rows[0];
List<IDataRow> children = current.InnerDataSet.GetChildRows("CustomerOrders", mergedCustomer);
Assert.Equal(2, children.Count);

bool contains11 = false;
for (int i = 0; i < children.Count; i++)
{
    if ((int)children[i]["OrderId"]! == 11)
    {
        contains11 = true;
        break;
    }
}

Assert.True(contains11);

        Assert.Empty(current.InnerDataSet.ValidateRelations());
    }
}
}
