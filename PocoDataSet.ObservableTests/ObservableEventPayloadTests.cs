using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableEventPayloadTests
    {
        #region Public Methods
        [Fact]
        public void DataFieldValueChanged_WhenRaised_ContainsCorrectColumnNameAndRequestor_AndSenderIsObservableRow()
        {
            // Arrange
            ObservableDataRow observableRow = CreateUnchangedObservableRowWithCount(1);

            DataFieldValueChangedCapture capture = new DataFieldValueChangedCapture();
            observableRow.DataFieldValueChanged += capture.Handler;

            // Act
            bool updated = observableRow.UpdateDataFieldValue("Count", 2, observableRow);

            // Assert
            Assert.True(updated);
            Assert.Equal(1, capture.Count);
            Assert.Same(observableRow, capture.Sender);
            Assert.NotNull(capture.Args);
            Assert.Equal("Count", capture.Args!.ColumnName);
            Assert.Same(observableRow, capture.Args!.Requestor);
        }

        [Fact]
        public void RowStateChanged_WhenRaised_ContainsOldAndNewState_AndRequestor()
        {
            // Arrange
            ObservableDataRow observableRow = CreateUnchangedObservableRowWithCount(1);

            RowStateChangedCapture capture = new RowStateChangedCapture();
            observableRow.RowStateChanged += capture.Handler;

            // Act
            bool updated = observableRow.UpdateDataFieldValue("Count", 2, observableRow);

            // Assert
            Assert.True(updated);
            Assert.Equal(1, capture.Count);
            Assert.Same(observableRow, capture.Sender);
            Assert.NotNull(capture.Args);
            Assert.Equal(PocoDataSet.IData.DataRowState.Unchanged, capture.Args!.OldState);
            Assert.Equal(PocoDataSet.IData.DataRowState.Modified, capture.Args!.NewState);
            Assert.Same(observableRow, capture.Args!.Requestor);
        }
        #endregion

        #region Private Helpers
        private static ObservableDataRow CreateUnchangedObservableRowWithCount(int count)
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");

            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Count", DataTypeNames.INT32);

            IDataRow row = table.AddNewRow();
            row["Id"] = 1;
            row["Count"] = count;

            table.AcceptChanges();

            return new ObservableDataRow((DataRow)row);
        }
        #endregion

        #region Private Types
        private sealed class DataFieldValueChangedCapture
        {
            public int Count { get; private set; }

            public object? Sender { get; private set; }

            public DataFieldValueChangedEventArgs? Args { get; private set; }

            public void Handler(object? sender, DataFieldValueChangedEventArgs e)
            {
                Count++;
                Sender = sender;
                Args = e;
            }
        }

        private sealed class RowStateChangedCapture
        {
            public int Count { get; private set; }

            public object? Sender { get; private set; }

            public RowStateChangedEventArgs? Args { get; private set; }

            public void Handler(object? sender, RowStateChangedEventArgs e)
            {
                Count++;
                Sender = sender;
                Args = e;
            }
        }
        #endregion
    }
}
