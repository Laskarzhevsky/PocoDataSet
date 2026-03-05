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
        /// ReplaceMode replaces all rows in observable table by rows from refreshed table
        /// </summary>
        [Fact]
        public void MergeWithTest_ReplaceMode()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            // Add row in Unchanged state to test row replacement when refreshed table has row with the same PK
            IObservableDataRow departmentObservableDataRow1 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow1["Id"] = 1;
            departmentObservableDataRow1["Name"] = "Sales";
            departmentObservableDataRow1.AcceptChanges();

            // Add row in Unchanged state to test row removal when refreshed table does not have row with the same PK
            IObservableDataRow departmentObservableDataRow11 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow11["Id"] = 11;
            departmentObservableDataRow11["Name"] = "Marketing";
            departmentObservableDataRow11.AcceptChanges();

            // Add row in Modified state to test row replacement when refreshed table has row with the same PK
            IObservableDataRow departmentObservableDataRow2 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow2["Id"] = 2;
            departmentObservableDataRow2["Name"] = "Sales";
            departmentObservableDataRow2.AcceptChanges();
            departmentObservableDataRow2["Name"] = "Emergency";

            // Add row in Modified state to test row removal when refreshed table does not have row with the same PK
            IObservableDataRow departmentObservableDataRow21 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow21["Id"] = 21;
            departmentObservableDataRow21["Name"] = "Sales";
            departmentObservableDataRow21.AcceptChanges();
            departmentObservableDataRow21["Name"] = "HR";

            // Add row in Added state to test row replacement when refreshed table has row with the same PK
            IObservableDataRow departmentObservableDataRow3 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow3["Id"] = 3;
            departmentObservableDataRow3["Name"] = "Reception";

            // Add row in Added state to test row removal when refreshed table does not have row with the same PK
            IObservableDataRow departmentObservableDataRow31 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow31["Id"] = 31;
            departmentObservableDataRow31["Name"] = "PR";

            // Add row in Deleted state to test row replacement when refreshed table has row with the same PK
            IObservableDataRow departmentObservableDataRow4 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow4["Id"] = 4;
            departmentObservableDataRow4["Name"] = "Finance";
            departmentObservableDataRow4.AcceptChanges();
            departmentObservableDataTable.DeleteRow(departmentObservableDataRow4);

            // Add row in Deleted state to test row removal when refreshed table does not have row with the same PK
            IObservableDataRow departmentObservableDataRow41 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow41["Id"] = 41;
            departmentObservableDataRow41["Name"] = "Finance";
            departmentObservableDataRow41.AcceptChanges();
            departmentObservableDataTable.DeleteRow(departmentObservableDataRow41);

            // 2. Create refreshed table snapshot
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartment = refreshedDataSet.AddNewTable("Department");
            refreshedDepartment.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartment.AddColumn("Name", DataTypeNames.STRING);

            // Add row to support the verification of the "Copy refreshed values" action
            IDataRow refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Sales Department";

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
            departmentObservableDataRow11.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow11.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow2.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow2.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow21.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow21.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow3.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow3.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow31.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow31.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow4.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow4.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow41.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow41.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;

            // Act
            // 3. Merge refreshed values into observable table using Replace mode
            IObservableMergeOptions observableMergeOptions = new ObservableMergeOptions();
            observableDataSet.DoReplaceMerge(refreshedDataSet, observableMergeOptions);

            // Assert
            Assert.Equal(4, departmentObservableDataTable.Rows.Count);

            // Assert the number of removed rows and added rows
            Assert.Equal(8, rowsRemovedEventHandler.GetEventCount());
            Assert.Equal(4, rowsAddedEventHandler.GetEventCount());

            // Verify that neither RowStateChanged nor DataFieldValueChanged events were raised
            Assert.Equal(0, rowStateChangedEventHandler.GetEventCount());
            Assert.Equal(0, dataFieldValueChangedEventHandler.GetEventCount());

            // Verify all rows are in Unchanged state and with correct data
            var refreshedObservableDataRow1 = FindRow(departmentObservableDataTable, 1);
            Assert.NotNull(refreshedObservableDataRow1);
            Assert.Equal("Sales Department", refreshedObservableDataRow1["Name"]);
            Assert.Equal(DataRowState.Unchanged, refreshedObservableDataRow1.DataRowState);

            var refreshedObservableDataRow2 = FindRow(departmentObservableDataTable, 2);
            Assert.NotNull(refreshedObservableDataRow2);
            Assert.Equal("Emergency Department", refreshedObservableDataRow2["Name"]);
            Assert.Equal(DataRowState.Unchanged, refreshedObservableDataRow2.DataRowState);

            var refreshedObservableDataRow3 = FindRow(departmentObservableDataTable, 3);
            Assert.NotNull(refreshedObservableDataRow3);
            Assert.Equal("Reception Department", refreshedObservableDataRow3["Name"]);
            Assert.Equal(DataRowState.Unchanged, refreshedObservableDataRow3.DataRowState);

            var refreshedObservableDataRow4 = FindRow(departmentObservableDataTable, 4);
            Assert.NotNull(refreshedObservableDataRow4);
            Assert.Equal("Finance Department", refreshedObservableDataRow4["Name"]);
            Assert.Equal(DataRowState.Unchanged, refreshedObservableDataRow4.DataRowState);

            // Verify that table subscribed to RowStateChanged and DataFieldValueChanged event of refreshed rows
            refreshedObservableDataRow1.RowStateChanged += rowStateChangedEventHandler.Handle;
            refreshedObservableDataRow1.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            refreshedObservableDataRow1["Name"] = "Sales";

            Assert.Equal(DataRowState.Modified, refreshedObservableDataRow1.DataRowState);
            Assert.Equal(1, rowStateChangedEventHandler.GetEventCount(refreshedObservableDataRow1, DataRowState.Unchanged, DataRowState.Modified));
            Assert.Equal(1, dataFieldValueChangedEventHandler.GetEventCount(refreshedObservableDataRow1, "Name"));
        }

        private static IObservableDataRow? FindRow(IObservableDataTable table, int id)
        {
            foreach (IObservableDataRow row in table.Rows)
            {
                if (Convert.ToInt32(row["Id"]) == id)
                {
                    return row;
                }
            }

            return null;
        }
    }
}
