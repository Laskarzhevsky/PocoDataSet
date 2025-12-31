using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableEventOrderingTests
    {
        #region Public Methods
        [Fact]
        public void UpdateDataFieldValue_WhenValueChanges_RaisesDataFieldValueChanged_BeforeRowStateChanged()
        {
            // Arrange
            ObservableDataRow observableRow = CreateUnchangedObservableRowWithCount(1);

            EventOrderRecorder recorder = new EventOrderRecorder();
            observableRow.DataFieldValueChanged += recorder.DataFieldValueChangedHandler;
            observableRow.RowStateChanged += recorder.RowStateChangedHandler;

            // Act
            bool updated = observableRow.UpdateDataFieldValue("Count", 2, null);

            // Assert
            Assert.True(updated);
            Assert.Equal(2, recorder.Events.Count);
            Assert.Equal(EventOrderRecorder.DATA_FIELD_VALUE_CHANGED, recorder.Events[0]);
            Assert.Equal(EventOrderRecorder.ROW_STATE_CHANGED, recorder.Events[1]);
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

            // Ensure row starts as Unchanged so that a field update transitions Unchanged -> Modified.
            table.AcceptChanges();

            return new ObservableDataRow((DataRow)row);
        }
        #endregion

        #region Private Types
        private sealed class EventOrderRecorder
        {
            public const string DATA_FIELD_VALUE_CHANGED = "DataFieldValueChanged";
            public const string ROW_STATE_CHANGED = "RowStateChanged";

            private readonly List<string> _events;

            public EventOrderRecorder()
            {
                _events = new List<string>();
            }

            public IList<string> Events
            {
                get { return _events; }
            }

            public void DataFieldValueChangedHandler(object? sender, DataFieldValueChangedEventArgs e)
            {
                _events.Add(DATA_FIELD_VALUE_CHANGED);
            }

            public void RowStateChangedHandler(object? sender, RowStateChangedEventArgs e)
            {
                _events.Add(ROW_STATE_CHANGED);
            }
        }
        #endregion
    }
}
