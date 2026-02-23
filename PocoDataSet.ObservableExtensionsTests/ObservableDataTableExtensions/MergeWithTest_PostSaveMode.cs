using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataTableExtensions
{
    public partial class ObservableDataTableExtensionsTests
    {
        [Fact]
        public void MergeWithTest_PostSaveMode()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.InnerDataTable.EnsureClientKeyColumnExists();
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);
            departmentObservableDataTable.AddColumn("RowVersion", DataTypeNames.BINARY);

            // a) Add row in Added state to verify "Propagate identity/rowversion/computed fields" action
            IObservableDataRow departmentObservableDataRow1 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow1["Name"] = "Engineering";

            // Correlation key (client-side) is typically used to match added rows with server-returned rows
            Guid clientKey = departmentObservableDataRow1.GetDataFieldValue<Guid>(SpecialColumnNames.CLIENT_KEY);

            // b) Add row in Modified state to verify "Propagate server-confirmed fields" action
            IObservableDataRow departmentObservableDataRow2 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow2["Id"] = 2;
            departmentObservableDataRow2["Name"] = "Sales";
            departmentObservableDataRow2.AcceptChanges();
            departmentObservableDataRow2["Name"] = "Emergency";

            // c) Add row in Deleted state to verify "Remove from table (finalize delete)" action
            // when rfreshed data table contains corresponded row
            IObservableDataRow departmentObservableDataRow3 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow3["Id"] = 3;
            departmentObservableDataRow3["Name"] = "Reception";
            departmentObservableDataRow3.AcceptChanges();
            departmentObservableDataTable.DeleteRow(departmentObservableDataRow3);

            // d) Add row in Deleted state to verify "Remove from table (finalize delete)" action
            // when rfreshed data table does not contain corresponded row
            IObservableDataRow departmentObservableDataRow31 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow31["Id"] = 31;
            departmentObservableDataRow31["Name"] = "PR";
            departmentObservableDataRow31.AcceptChanges();
            departmentObservableDataTable.DeleteRow(departmentObservableDataRow31);

            // e) Add row in Unchanged state to verify "May update server fields (no harm)" action
            IObservableDataRow departmentObservableDataRow4 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow4["Id"] = 4;
            departmentObservableDataRow4["Name"] = "Financial";
            departmentObservableDataRow4.AcceptChanges();

            // 2. Create a refreshed table containing server-generated values
            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.EnsureClientKeyColumnExists();
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            // Add row to support the verification of the "Propagate identity/rowversion/computed fields" action
            IDataRow postSaveRowData = postSaveDepartment.AddNewRow();
            postSaveRowData["Id"] = 1;
            postSaveRowData["Name"] = "Engineering";
            postSaveRowData["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };
            postSaveRowData[SpecialColumnNames.CLIENT_KEY] = clientKey; // correlate rows

            // Add row to support the verification of the "Propagate server-confirmed fields" action
            postSaveRowData = postSaveDepartment.AddNewRow();
            postSaveRowData["Id"] = 2;
            postSaveRowData["Name"] = "Emergency";

            // Add row to support the verification of the "Remove from table (finalize delete)" action
            postSaveRowData = postSaveDepartment.AddNewRow();
            postSaveRowData["Id"] = 3;
            postSaveRowData["Name"] = "Reception";

            // Add row to support the verification of the "May update server fields (no harm)" action
            postSaveRowData = postSaveDepartment.AddNewRow();
            postSaveRowData["Id"] = 4;
            postSaveRowData["Name"] = "Finance";

            // Event counters
            DataFieldValueChangedEventHandler dataFieldValueChangedEventHandler = new DataFieldValueChangedEventHandler();
            RowsAddedEventHandler rowsAddedEventHandler = new RowsAddedEventHandler();
            RowsRemovedEventHandler rowsRemovedEventHandler = new RowsRemovedEventHandler();
            RowStateChangedEventHandler rowStateChangedEventHandler = new RowStateChangedEventHandler();

            departmentObservableDataTable.RowsAdded += rowsAddedEventHandler.Handle;
            departmentObservableDataTable.RowsRemoved += rowsRemovedEventHandler.Handle;

            departmentObservableDataRow1.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow1.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow2.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow2.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow3.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow31.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow4.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow4.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;

            // Act
            // 3. Merge server-returned values into current table using PostSave mode
            IObservableMergeOptions observableMergeOptions = new ObservableMergeOptions();
            observableDataSet.DoPostSaveMerge(postSaveDataSet, observableMergeOptions);

            // Assert
            // Three rows left after merge
            Assert.Equal(3, departmentObservableDataTable.Rows.Count);

            // Row in Added state had been merged with refreshed data
            // Row now has server-generated values (identity/rowversion) and is typically accepted as Unchanged by the merge handlers
            int id = departmentObservableDataRow1.GetDataFieldValue<int>("Id"); // 1
            byte[]? rowVersion = departmentObservableDataRow1.GetDataFieldValue<byte[]>("RowVersion");
            Assert.Equal(1, id);
            Assert.NotNull(rowVersion);
            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, rowVersion);
            Assert.Equal(DataRowState.Unchanged, departmentObservableDataRow1.DataRowState);
            Assert.Equal(1, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow1));
            Assert.Equal(1, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow1, "Id"));
            Assert.Equal(0, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow1, "Name"));

            // Row in Modified state had been merged with refreshed data
            Assert.Equal(1, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow2, DataRowState.Modified, DataRowState.Unchanged));
            Assert.Equal(2, departmentObservableDataRow2["Id"]);
            Assert.Equal("Emergency", departmentObservableDataRow2["Name"]);
            Assert.Equal(0, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow2, "Id"));
            Assert.Equal(0, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow2, "Name"));

            // Rows in Deleted were removed
            Assert.Equal(1, rowsRemovedEventHandler.GetEventCount(departmentObservableDataRow3));
            Assert.Equal(1, rowsRemovedEventHandler.GetEventCount(departmentObservableDataRow31));

            // Row in Unchanged state had been merged with refreshed data
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow4));
            Assert.Equal(4, departmentObservableDataRow4["Id"]);
            Assert.Equal("Finance", departmentObservableDataRow4["Name"]);
            Assert.Equal(0, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow4, "Id"));
            Assert.Equal(1, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow4, "Name"));
        }
    }
}
