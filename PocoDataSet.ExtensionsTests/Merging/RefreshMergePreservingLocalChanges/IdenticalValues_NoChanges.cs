using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the "no-op refresh" contract for RefreshPreservingLocalChanges: if refreshed values
    /// are identical to current values, the merge must not create churn (no row replacement, no
    /// state changes, no original values).
    ///
    /// Scenario:
    /// - Build a CURRENT DataSet (the client-side truth before the merge).
    /// - Build a REFRESHED/CHANGESET DataSet (the server-side snapshot or post-save response).
    /// - Execute RefreshPreservingLocalChanges merge (refresh unchanged rows, preserve local changes).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
    /// Notes:
    /// - This file contains exactly one test method: IdenticalValues_NoChanges.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void IdenticalValues_NoChanges()
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
            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
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
