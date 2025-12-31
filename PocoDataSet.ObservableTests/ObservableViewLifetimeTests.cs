using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableViewLifetimeTests
    {
        #region Public Methods
        [Fact]
        public void DisposingView_UnsubscribesFromTableEvents_AndStopsRaisingRowsAdded()
        {
            // Arrange
            ObservableDataSet observableDataSet = CreateObservableDataSet(out IDataTable innerTable);
            IObservableDataView view = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenA");

            RowsChangedCounter rowsAddedCounter = new RowsChangedCounter();
            view.RowsAdded += rowsAddedCounter.Handler;

            // Sanity
            Assert.Equal(2, view.Rows.Count);

            // Act: dispose view
            view.Dispose();

            // Add a new row and notify observable layer.
            IDataRow newRow = innerTable.AddNewRow();
            newRow["Id"] = 3;
            newRow["Name"] = "IT";
            observableDataSet.Tables["Department"].AddRow(newRow);

            // Still disposed: no events and still empty
            Assert.Equal(0, rowsAddedCounter.Count);
            Assert.Equal(0, view.Rows.Count);
        }

        [Fact]
        public void DisposingOneView_DoesNotAffectAnotherView_OverSameTable()
        {
            // Arrange
            ObservableDataSet observableDataSet = CreateObservableDataSet(out IDataTable innerTable);

            IObservableDataView viewA = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenA");
            IObservableDataView viewB = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenB");

            RowsChangedCounter rowsAddedCounterB = new RowsChangedCounter();
            viewB.RowsAdded += rowsAddedCounterB.Handler;

            // Act
            viewA.Dispose();

            IDataRow newRow = innerTable.AddNewRow();
            newRow["Id"] = 3;
            newRow["Name"] = "IT";
            observableDataSet.Tables["Department"].AddRow(newRow);

            // Assert
            Assert.Equal(1, rowsAddedCounterB.Count);
            Assert.Equal(3, viewB.Rows.Count);
        }
        #endregion

        #region Private Helpers
        private static ObservableDataSet CreateObservableDataSet(out IDataTable innerTable)
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            innerTable = dataSet.AddNewTable("Department");

            innerTable.AddColumn("Id", DataTypeNames.INT32);
            innerTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row1 = innerTable.AddNewRow();
            row1["Id"] = 1;
            row1["Name"] = "Sales";

            IDataRow row2 = innerTable.AddNewRow();
            row2["Id"] = 2;
            row2["Name"] = "HR";

            innerTable.AcceptChanges();

            return new ObservableDataSet((DataSet)dataSet);
        }
        #endregion
    }
}
