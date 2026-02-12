using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.ObservableData;
using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableAcceptRejectIntegrationTests
    {
        #region Public Methods
        [Fact]
        public void AcceptChanges_WhenCalledOnInnerTable_UpdatesObservableRowState_ButDoesNotRaiseRowStateChangedEvent()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            DataRow row = new DataRow();
            row["Id"] = 1;
            row["Name"] = "Sales";
            table.AddRow(row);

           // Make the row "persisted" so we can observe Unchanged -> Modified transitions.
            table.AcceptChanges();

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);
            var observableTable = observableDataSet.Tables["Department"];
            var observableRow = observableTable.Rows[0];

            RowStateChangedCounter stateCounter = new RowStateChangedCounter();
            observableRow.RowStateChanged += stateCounter.Handler;

            // Act (make row modified through observable API)
            bool changed = observableRow.UpdateDataFieldValue("Name", "Sales2", this);

            // Assert (pre)
            Assert.True(changed);
            Assert.Equal(1, stateCounter.Count);
            Assert.Equal(PocoDataSet.IData.DataRowState.Modified, observableRow.DataRowState);

            // Act (accept changes on inner table)
            table.AcceptChanges();

            // Assert (state is updated, but observable does not raise an event for AcceptChanges)
            Assert.Equal(PocoDataSet.IData.DataRowState.Unchanged, observableRow.DataRowState);
            Assert.Equal(1, stateCounter.Count);
        }

        [Fact]
        public void RejectChanges_WhenCalledOnInnerTable_RevertsValueAndState_ButDoesNotRaiseDataFieldOrRowStateEvents()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            DataRow row = new DataRow();
            row["Id"] = 1;
            row["Name"] = "Sales";
            table.AddRow(row);

           // Make the row "persisted" so we can observe Unchanged -> Modified transitions.
            table.AcceptChanges();

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);
            var observableTable = observableDataSet.Tables["Department"];
            var observableRow = observableTable.Rows[0];

            DataFieldValueChangedCounter dataFieldCounter = new DataFieldValueChangedCounter();
            RowStateChangedCounter stateCounter = new RowStateChangedCounter();

            observableRow.DataFieldValueChanged += dataFieldCounter.Handler;
            observableRow.RowStateChanged += stateCounter.Handler;

            // Act
            bool changed = observableRow.UpdateDataFieldValue("Name", "Sales2", this);
            Assert.True(changed);
            Assert.Equal(1, dataFieldCounter.Count);
            Assert.Equal(1, stateCounter.Count);
            Assert.Equal(PocoDataSet.IData.DataRowState.Modified, observableRow.DataRowState);

            table.RejectChanges();

            // Assert: value and state revert
            Assert.Equal("Sales", observableRow["Name"]);
            Assert.Equal(PocoDataSet.IData.DataRowState.Unchanged, observableRow.DataRowState);

            // And no extra events were raised by RejectChanges
            Assert.Equal(1, dataFieldCounter.Count);
            Assert.Equal(1, stateCounter.Count);
        }
        #endregion
    }
}
