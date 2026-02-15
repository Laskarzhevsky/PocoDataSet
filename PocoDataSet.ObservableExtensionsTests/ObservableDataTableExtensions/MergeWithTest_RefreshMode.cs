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
        /// <summary>
        /// RefreshMode preserves local changes
        /// </summary>
        [Fact]
        public void MergeWithTest_RefreshMode()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            // Add row in Unchanged state to verify "Copy refreshed values" action
            // Will be kept after refresh
            IObservableDataRow departmentObservableDataRow1 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow1["Id"] = 1;
            departmentObservableDataRow1["Name"] = "Sales";
            departmentObservableDataRow1.AcceptChanges();

            // Add row in Modified state to verify "Preserve local edits" action
            IObservableDataRow departmentObservableDataRow2 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow2["Id"] = 2;
            departmentObservableDataRow2["Name"] = "Sales";
            departmentObservableDataRow2.AcceptChanges();
            departmentObservableDataRow2["Name"] = "Emergency";

            // Add row in Added state to verify "Preserve local new row" action
            IObservableDataRow departmentObservableDataRow3 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow3["Id"] = 3;
            departmentObservableDataRow3["Name"] = "Reception";

            // Add row in Deleted state to verify "Preserve pending delete" action
            IObservableDataRow departmentObservableDataRow4 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow4["Id"] = 4;
            departmentObservableDataRow4["Name"] = "Finance";
            departmentObservableDataRow4.AcceptChanges();
            departmentObservableDataTable.DeleteRow(departmentObservableDataRow4);

            // Add row in Unchanged state to verify "Remove row (unless table excluded from deletion)" action
            // Will be removed after refresh
            IObservableDataRow departmentObservableDataRow11 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow11["Id"] = 11;
            departmentObservableDataRow11["Name"] = "Sales";
            departmentObservableDataRow11.AcceptChanges();

            // Add row in Unchanged state to verify that it will not be merged because of missing data in refresh
            IObservableDataRow departmentObservableDataRow12 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow12["Id"] = 12;
            departmentObservableDataRow12["Name"] = "PR";
            departmentObservableDataRow12.AcceptChanges();

            // Add row in Modified state to verify that it will not be merged because of missing data in refresh
            IObservableDataRow departmentObservableDataRow22 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow22["Id"] = 22;
            departmentObservableDataRow22["Name"] = "Sales";
            departmentObservableDataRow22.AcceptChanges();
            departmentObservableDataRow22["Name"] = "HR";

            // 2. Create refreshed table snapshot
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartment = refreshedDataSet.AddNewTable("Department");
            refreshedDepartment.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartment.AddColumn("Name", DataTypeNames.STRING);

            // Add row to support the verification of the "Copy refreshed values" action
            IDataRow refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Sales and Marketing";

            // Add row to support the verification of the "Preserve local edits" action
            refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 2;
            refreshedRow["Name"] = "Emergency Department";

            // Add row to support the verification of the "Preserve local new row" action
            refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 3;
            refreshedRow["Name"] = "Reception Department";

            // Add row to support the verification of the "Preserve pending delete" action
            refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 4;
            refreshedRow["Name"] = "Finance Department";

            // Add row to verify "Adds new rows from refreshed as Unchanged" rule
            refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 21;
            refreshedRow["Name"] = "Management";

            refreshedDataSet.AcceptChanges();

            // Event counters
            DataFieldValueChangedEventHandler dataFieldValueChangedEventHandler = new DataFieldValueChangedEventHandler();
            RowsAddedEventHandler rowsAddedEventHandler = new RowsAddedEventHandler();
            RowsRemovedEventHandler rowsRemovedEventHandler = new RowsRemovedEventHandler();
            RowStateChangedEventHandler rowStateChangedEventHandler = new RowStateChangedEventHandler();

            departmentObservableDataTable.RowsAdded += rowsAddedEventHandler.Handle;
            departmentObservableDataTable.RowsRemoved += rowsRemovedEventHandler.Handle;
            departmentObservableDataRow1.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow1.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow12.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow12.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow2.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow2.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow22.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow22.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow3.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow3.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;

            // Act
            // 3. Merge refreshed values into observable table using Refresh mode
            IObservableMergeOptions observableMergeOptions = new ObservableMergeOptions();
            observableMergeOptions.MergeMode = MergeMode.RefreshPreservingLocalChanges;
            departmentObservableDataTable.MergeWith(refreshedDepartment, observableMergeOptions);

            // Assert
            // 4. Row in Unchanged state has refreshed values
            string? name = departmentObservableDataTable.GetFieldValue<string>(0, "Name");
            Assert.Equal("Sales and Marketing", name);
            Assert.Equal(DataRowState.Unchanged, departmentObservableDataRow1.DataRowState);
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow1));
            Assert.Equal(1, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow1, "Name"));

            // Row in Unchanged was removed because there is no corresponding refreshed data
            Assert.Equal(1, rowsRemovedEventHandler.GetEventCount(departmentObservableDataRow11));
            Assert.Equal(0, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow11));

            // Row in Modified state had not been merged with refreshed data
            name = departmentObservableDataTable.GetFieldValue<string>(1, "Name");
            Assert.Equal("Emergency", name);
            Assert.Equal(DataRowState.Modified, departmentObservableDataRow2.DataRowState);
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow2));
            Assert.Equal(0, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow2));

            // Row in Added state had not been merged with refreshed data
            name = departmentObservableDataTable.GetFieldValue<string>(2, "Name");
            Assert.Equal("Reception", name);
            Assert.Equal(DataRowState.Added, departmentObservableDataRow3.DataRowState);
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow3));
            Assert.Equal(0, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow3));

            // Row in Deleted state had not been merged with refreshed data
            name = departmentObservableDataTable.GetFieldValue<string>(3, "Name");
            Assert.Equal("Finance", name);
            Assert.Equal(DataRowState.Deleted, departmentObservableDataRow4.DataRowState);
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow4));
            Assert.Equal(0, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow4));

            // Department table has 6 rows: 4 original and 1 added
            Assert.Equal(6, departmentObservableDataTable.Rows.Count);
            Assert.Equal(1, rowsAddedEventHandler.GetEventCount());

            // Assert: refreshed-only row was added and ends as Unchanged
            IObservableDataRow? addedRow;
            bool wasFound = departmentObservableDataTable.TryFindRowByPrimaryKey("Id", 21, out addedRow);

            Assert.True(wasFound);
            Assert.NotNull(addedRow);
            Assert.Equal("Management", addedRow!.GetDataFieldValue<string>("Name"));
            Assert.Equal(DataRowState.Unchanged, addedRow.DataRowState);

            // Rows are not merged because of missing data in refreshed data set
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow12));
            Assert.Equal(0, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow12));
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow22));
            Assert.Equal(0, dataFieldValueChangedEventHandler.GetEventCount(departmentObservableDataRow22));
        }
    }
}
