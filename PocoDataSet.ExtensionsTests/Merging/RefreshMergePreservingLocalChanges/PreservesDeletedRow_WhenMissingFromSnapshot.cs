using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies that a locally Deleted row remains Deleted after *RefreshPreservingLocalChanges* when the refreshed
        /// snapshot omits it.  Scenario: - Client has already marked a row Deleted (pending delete). - Refreshed
        /// snapshot does not contain the row (server may already be missing it, or snapshot is incomplete).  Expected
        /// behavior: - The local Deleted intent is preserved (do not resurrect the row). - The Deleted row remains
        /// Deleted / tracked appropriately.
        /// </summary>

        [Fact]
        public void PreservesDeletedRow_WhenMissingFromSnapshot()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 1;
            row["Name"] = "ToDelete";
            t.AddLoadedRow(row);

            t.DeleteRow(row);
            Assert.Equal(DataRowState.Deleted, row.DataRowState);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);

            // Act
            // Merge options are part of the contract surface; using defaults here exercises the standard behavior.
            MergeOptions options = new MergeOptions();
            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            // Deleted row preserved in Refresh mode
            Assert.Equal(1, t.Rows.Count);
            Assert.Equal(DataRowState.Deleted, t.Rows[0].DataRowState);
        }
    }
}
