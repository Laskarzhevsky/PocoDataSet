using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies WhenValuesIdentical RaisesNoCellEvents AndKeepsRowInstance in RefreshMergePreservingLocalChanges merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void ValuesIdentical_RaisesNoCellEvents_KeepsInstance()
        {
            // ------------------------------------------------------------
            // Arrange
            // ------------------------------------------------------------
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow currentRow = currentTable.AddNewRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Sales";
            currentRow.AcceptChanges();

            int dataFieldChangedCount = 0;

            void OnDataFieldChanged(object? sender, DataFieldValueChangedEventArgs e)
            {
                dataFieldChangedCount++;
            }

            currentRow.DataFieldValueChanged += OnDataFieldChanged;

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Sales";
            refreshedTable.AddLoadedRow(refreshedRow);

            // ------------------------------------------------------------
            // Act
            // ------------------------------------------------------------
            IObservableMergeOptions options = new ObservableMergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // ------------------------------------------------------------
            // Assert
            // ------------------------------------------------------------
            Assert.Single(currentTable.Rows);
            Assert.Same(currentRow, currentTable.Rows[0]);
            Assert.Equal(0, dataFieldChangedCount);

            Assert.Equal("Sales", (string)currentRow["Name"]!);
            Assert.Equal(DataRowState.Unchanged, currentRow.InnerDataRow.DataRowState);
            Assert.False(currentRow.InnerDataRow.HasOriginalValues);

            currentRow.DataFieldValueChanged -= OnDataFieldChanged;
        }
    }
}