using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// Locks the "no-op refresh" contract for RefreshPreservingLocalChanges:
    /// if refreshed values are identical to current values, the merge must not create churn
    /// (no row replacement, no state changes, no original values).
    /// </summary>
    public class RefreshNoOpStabilityTests
    {
        [Fact]
        public void RefreshPreservingLocalChanges_WhenValuesIdentical_DoesNotChangeRowOrState()
        {
            // ------------------------------------------------------------
            // Arrange
            // ------------------------------------------------------------
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            currentRow["Id"] = 1;
            currentRow["Name"] = "Sales";
            currentTable.AddLoadedRow(currentRow);

            Assert.Equal(DataRowState.Unchanged, currentRow.DataRowState);
            Assert.False(currentRow.HasOriginalValues);

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
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // ------------------------------------------------------------
            // Assert
            // ------------------------------------------------------------
            Assert.Single(currentTable.Rows);
            Assert.Same(currentRow, currentTable.Rows[0]);

            Assert.Equal(DataRowState.Unchanged, currentRow.DataRowState);
            Assert.False(currentRow.HasOriginalValues);
            Assert.Equal("Sales", (string)currentRow["Name"]!);
        }
    }
}
