using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies that *RefreshPreservingLocalChanges* updates rows that match by PK and tracks the update
        /// appropriately.  Scenario: - Current has an Unchanged row. - Refreshed snapshot has the same PK with
        /// different non-key values.  Expected behavior: - Current row values are updated to the refreshed values. -
        /// The row state is tracked as Updated/Modified according to the merge result policy (then often becomes
        /// Unchanged as snapshot truth).
        /// </summary>

        [Fact]
        public void UpdatesMatchingRows_TracksUpdated()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            currentRow["Id"] = 2;
            currentRow["Name"] = "Financial";
            currentTable.AddLoadedRow(currentRow);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            refreshedRow["Id"] = 2;
            refreshedRow["Name"] = "Reception";
            refreshedTable.AddLoadedRow(refreshedRow);

            // Act
            // Merge options are part of the contract surface; using defaults here exercises the standard behavior.
            MergeOptions options = new MergeOptions();
            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Assert
            Assert.Single(currentTable.Rows);
            Assert.Equal(2, currentTable.Rows[0]["Id"]);
            Assert.Equal("Reception", currentTable.Rows[0]["Name"]);

            // Refresh merge treats refreshed data as baseline
            Assert.Equal(DataRowState.Unchanged, currentTable.Rows[0].DataRowState);

            // The merge result should record an update
            Assert.Single(result.UpdatedDataRows);
            Assert.Empty(result.AddedDataRows);
            Assert.Empty(result.DeletedDataRows);
        }
    }
}
