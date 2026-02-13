using System;

using PocoDataSet.Data;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableDataRowTests
    {
        [Fact]
        public void UpdateDataFieldValue_WhenValueEqualsOriginal_ReturnsFalse_AndDoesNotRaiseEvents()
        {
            // Arrange
            DataRow innerRow = CreateLoadedRowWithCount(1);
            ObservableDataRow observableRow = new ObservableDataRow(innerRow);

            DataFieldValueChangedCounter dataFieldCounter = new DataFieldValueChangedCounter();
            RowStateChangedCounter rowStateCounter = new RowStateChangedCounter();

            observableRow.DataFieldValueChanged += dataFieldCounter.Handler;
            observableRow.RowStateChanged += rowStateCounter.Handler;

            object boxedOne = 1; // different boxed instance, same numeric value

            // Act
            observableRow["Count"] = boxedOne;

            // Assert
            Assert.Equal(0, dataFieldCounter.Count);
            Assert.Equal(0, rowStateCounter.Count);
            Assert.Equal(PocoDataSet.IData.DataRowState.Unchanged, observableRow.DataRowState);
        }

        [Fact]
        public void Indexer_WhenValueDiffersFromOriginal_RaisesEvents_AndMarksRowModified()
        {
            // Arrange
            DataRow innerRow = CreateLoadedRowWithCount(1);
            ObservableDataRow observableRow = new ObservableDataRow(innerRow);

            DataFieldValueChangedCounter dataFieldCounter = new DataFieldValueChangedCounter();
            RowStateChangedCounter rowStateCounter = new RowStateChangedCounter();

            observableRow.DataFieldValueChanged += dataFieldCounter.Handler;
            observableRow.RowStateChanged += rowStateCounter.Handler;

            // Act
            observableRow["Count"] = 2;

            // Assert
            Assert.Equal(1, dataFieldCounter.Count);
            Assert.Equal(1, rowStateCounter.Count);
            Assert.Equal(PocoDataSet.IData.DataRowState.Modified, observableRow.DataRowState);
        }

        [Fact]
        public void Indexer_WhenSameValueIsAssignedTwice_RaisesEventsOnlyOnce()
        {
            // Arrange
            DataRow innerRow = CreateLoadedRowWithCount(1);
            ObservableDataRow observableRow = new ObservableDataRow(innerRow);

            DataFieldValueChangedCounter dataFieldCounter = new DataFieldValueChangedCounter();
            RowStateChangedCounter rowStateCounter = new RowStateChangedCounter();

            observableRow.DataFieldValueChanged += dataFieldCounter.Handler;
            observableRow.RowStateChanged += rowStateCounter.Handler;

            // Act
            observableRow["Count"] = 2;
            observableRow["Count"] = 2;

            // Assert
            Assert.Equal(1, dataFieldCounter.Count);
            Assert.Equal(1, rowStateCounter.Count);
            Assert.Equal(PocoDataSet.IData.DataRowState.Modified, observableRow.DataRowState);
        }

        [Fact]
        public void Indexer_WhenValueChangesBackToOriginal_DoesNotRaiseEvents_ButRowRemainsModified()
        {
            // Arrange
            DataRow innerRow = CreateLoadedRowWithCount(1);
            ObservableDataRow observableRow = new ObservableDataRow(innerRow);

            DataFieldValueChangedCounter dataFieldCounter = new DataFieldValueChangedCounter();
            RowStateChangedCounter rowStateCounter = new RowStateChangedCounter();

            observableRow.DataFieldValueChanged += dataFieldCounter.Handler;
            observableRow.RowStateChanged += rowStateCounter.Handler;

            // Act
            observableRow["Count"] = 2; // change away from original
            observableRow["Count"] = 1; // back to original

            // Assert
            Assert.Equal(1, dataFieldCounter.Count);
            Assert.Equal(1, rowStateCounter.Count);
            Assert.Equal(1, observableRow.GetDataFieldValue<int>("Count"));
            Assert.Equal(PocoDataSet.IData.DataRowState.Modified, observableRow.DataRowState);
        }

        [Fact]
        public void Indexer_WhenColumnDoesNotExist_AddsValueAndRaisesEvents()
        {
            // Arrange
            DataRow innerRow = CreateLoadedRowWithCount(1);
            ObservableDataRow observableRow = new ObservableDataRow(innerRow);

            DataFieldValueChangedCounter dataFieldCounter = new DataFieldValueChangedCounter();
            RowStateChangedCounter rowStateCounter = new RowStateChangedCounter();

            observableRow.DataFieldValueChanged += dataFieldCounter.Handler;
            observableRow.RowStateChanged += rowStateCounter.Handler;

            // Act
            observableRow["NewColumn"] = "Hello";

            // Assert
            Assert.Equal("Hello", observableRow["NewColumn"]);
            Assert.Equal(1, dataFieldCounter.Count);
            Assert.Equal(1, rowStateCounter.Count);
            Assert.Equal(PocoDataSet.IData.DataRowState.Modified, observableRow.DataRowState);
        }

        private static DataRow CreateLoadedRowWithCount(int count)
        {
            DataTable table = new DataTable();

            ColumnMetadata countColumn = new ColumnMetadata();
            countColumn.ColumnName = "Count";
            countColumn.DataType = DataTypeNames.INT32;

            table.AddColumn(countColumn);

            DataRow row = new DataRow();
            row["Count"] = count;

            // Add as loaded row so it becomes Unchanged baseline (and captures original values on first edit)
            table.AddLoadedRow(row);

            return row;
        }

        private class RowStateChangedCounter
        {
            public int Count
            {
                get; private set;
            }

            public void Handler(object? sender, RowStateChangedEventArgs e)
            {
                Count++;
            }
        }
    }
}
