using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    /// <summary>
    /// Locks the "no-op refresh" contract for Observable RefreshPreservingLocalChanges:
    /// if refreshed values are identical, the merge must not raise DataFieldValueChanged
    /// and must not replace the observable row instance.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void WhenValuesIdentical_RaisesNoCellEvents_AndKeepsRowInstance()
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
