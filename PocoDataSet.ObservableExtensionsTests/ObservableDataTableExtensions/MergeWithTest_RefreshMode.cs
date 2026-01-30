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

            // Add row in Unchanged state
            IObservableDataRow departmentObservableDataRow1 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow1["Id"] = 1;
            departmentObservableDataRow1["Name"] = "Sales";
            departmentObservableDataRow1.AcceptChanges();

            // Add row in Modified state
            IObservableDataRow departmentObservableDataRow2 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow2["Id"] = 2;
            departmentObservableDataRow2["Name"] = "Sales";
            departmentObservableDataRow2.AcceptChanges();
            departmentObservableDataRow2["Name"] = "Emergency";

            // Add row in Added state
            IObservableDataRow departmentObservableDataRow3 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow3["Id"] = 3;
            departmentObservableDataRow3["Name"] = "Reception";

            // Add row in Deleted state
            IObservableDataRow departmentObservableDataRow4 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow4["Id"] = 4;
            departmentObservableDataRow4["Name"] = "Finance";
            departmentObservableDataRow4.AcceptChanges();
            departmentObservableDataTable.DeleteRow(departmentObservableDataRow4);

            // 2. Create refreshed table snapshot
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartment = refreshedDataSet.AddNewTable("Department");
            refreshedDepartment.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Sales and Marketing";

            refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 2;
            refreshedRow["Name"] = "Emergency Department";

            refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 3;
            refreshedRow["Name"] = "Reception Department";

            refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 4;
            refreshedRow["Name"] = "Finance Department";

            refreshedDataSet.AcceptChanges();

            // Event counters
            DataFieldValueChangedEventHandler dataFieldValueChangedEventHandler = new DataFieldValueChangedEventHandler();
            departmentObservableDataRow1.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;

            RowsRemovedEventHandler rowsRemovedEventHandler = new RowsRemovedEventHandler();
            RowStateChangedEventHandler rowStateChangedEventHandler = new RowStateChangedEventHandler();

            departmentObservableDataTable.RowsRemoved += rowsRemovedEventHandler.Handle;
            departmentObservableDataRow1.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow2.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow3.RowStateChanged += rowStateChangedEventHandler.Handle;

            // Act
            // 3. Merge refreshed values into observable table using Refresh mode
            IObservableMergeOptions observableMergeOptions = new ObservableMergeOptions();
            observableMergeOptions.MergeMode = MergeMode.Refresh;
            departmentObservableDataTable.MergeWith(refreshedDepartment, observableMergeOptions);

            // Assert
            // 4. Row in Unchanged state has refreshed values
            string? name = departmentObservableDataTable.GetFieldValue<string>(0, "Name");
            Assert.Equal("Sales and Marketing", name);
            Assert.Equal(DataRowState.Unchanged, departmentObservableDataRow1.DataRowState);
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow1));
            Assert.Equal(1, dataFieldValueChangedEventHandler.GetEventCount("Name"));

            // Row in Modified state had not been merged with refreshed data
            name = departmentObservableDataTable.GetFieldValue<string>(1, "Name");
            Assert.Equal("Emergency", name);
            Assert.Equal(DataRowState.Modified, departmentObservableDataRow2.DataRowState);
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow2));

            // Row in Added state had not been merged with refreshed data
            name = departmentObservableDataTable.GetFieldValue<string>(2, "Name");
            Assert.Equal("Reception", name);
            Assert.Equal(DataRowState.Added, departmentObservableDataRow3.DataRowState);
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow3));

            // Row in Deleted state had not been merged with refreshed data
            name = departmentObservableDataTable.GetFieldValue<string>(3, "Name");
            Assert.Equal("Finance", name);
            Assert.Equal(DataRowState.Deleted, departmentObservableDataRow4.DataRowState);
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow4));

            // Department table still has 4 rows
            Assert.Equal(4, departmentObservableDataTable.Rows.Count);
        }
    }
}
