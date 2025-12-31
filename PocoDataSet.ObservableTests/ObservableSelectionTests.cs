using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableSelectionTests
    {
        #region Public Methods
        [Fact]
        public void SettingSelected_OnObservableRow_RaisesDataFieldValueChanged_AndRowStateChanged()
        {
            // Arrange
            IObservableDataView view = CreateObservableViewWithSelection();
            IObservableDataRow row = view.Rows[0];

            DataFieldValueChangedCounter dataFieldCounter = new DataFieldValueChangedCounter();
            RowStateChangedCounter rowStateCounter = new RowStateChangedCounter();

            row.DataFieldValueChanged += dataFieldCounter.Handler;
            row.RowStateChanged += rowStateCounter.Handler;

            // Act
            bool updated = row.UpdateSelectedDataFieldValue(true, null);

            // Assert
            Assert.True(updated);
            Assert.Equal(1, dataFieldCounter.Count);
            Assert.Equal(1, rowStateCounter.Count);
            Assert.True(row.GetDataFieldValue<bool>(ColumnNames.SELECTED) == true);
        }

        [Fact]
        public void RemovingSelectedRow_FromTable_DoesNotTransferSelectionToAnotherRow()
        {
            // Arrange
            IDataTable innerTable;
            ObservableDataSet observableDataSet = CreateObservableDataSetWithSelection(out innerTable);

            IObservableDataView view = observableDataSet.GetObservableDataView("Department", null, false, null, "SelectionTest");
            IObservableDataTable observableTable = observableDataSet.Tables["Department"];

            // Select first row (however your selection is set)
            view.Rows[0].UpdateDataFieldValue(ColumnNames.SELECTED, true, null);

            // Act: remove the selected row via observable table (NOT innerTable)
            observableTable.RemoveRow(0);

            // Assert: view should now have 1 row left
            Assert.Single(view.Rows);

            // And selection did not “transfer” to the remaining row
            bool isSelected = view.Rows[0].InnerDataRow.GetDataFieldValue<bool>(ColumnNames.SELECTED);
            Assert.False(isSelected);
        }
        #endregion

        #region Private Helpers
        private static IObservableDataView CreateObservableViewWithSelection()
        {
            ObservableDataSet observableDataSet = CreateObservableDataSetWithSelection(out IDataTable _);
            return observableDataSet.GetObservableDataView("Department", null, false, null, "Screen");
        }

        private static ObservableDataSet CreateObservableDataSetWithSelection(out IDataTable innerTable)
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            innerTable = dataSet.AddNewTable("Department");

            innerTable.AddColumn("Id", DataTypeNames.INT32);
            innerTable.AddColumn("Name", DataTypeNames.STRING);
            innerTable.AddColumn(ColumnNames.SELECTED, DataTypeNames.BOOL);

            IDataRow row1 = innerTable.AddNewRow();
            row1["Id"] = 1;
            row1["Name"] = "Sales";
            row1[ColumnNames.SELECTED] = false;

            IDataRow row2 = innerTable.AddNewRow();
            row2["Id"] = 2;
            row2["Name"] = "HR";
            row2[ColumnNames.SELECTED] = false;

            innerTable.AcceptChanges();

            return new ObservableDataSet((DataSet)dataSet);
        }
        #endregion
    }
}
