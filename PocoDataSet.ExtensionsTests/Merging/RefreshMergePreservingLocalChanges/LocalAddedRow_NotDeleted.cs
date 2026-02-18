using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies that *RefreshPreservingLocalChanges* does **not delete locally Added rows** that do not appear in
        /// the refreshed snapshot.  Scenario: - Current contains a locally Added row (pending insert). - Refreshed
        /// snapshot (server state) naturally does not contain that row yet.  Expected behavior: - The Added row is
        /// preserved as Added (pending local change is kept). - The merge only deletes missing rows that are in-sync
        /// (typically Unchanged), not pending local inserts.
        /// </summary>

        [Fact]
        public void LocalAddedRow_NotDeleted()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            // Baseline row (loaded from server earlier)
            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            loaded["Id"] = 1;
            loaded["Name"] = "A";
            currentTable.AddLoadedRow(loaded);

            // Local client-added row (must not be removed by Refresh merge)
            IDataRow added = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            added["Id"] = 999;
            added["Name"] = "LOCAL";
            currentTable.AddRow(added);

            Assert.Equal(DataRowState.Added, added.DataRowState);

            // Refreshed snapshot contains only the baseline row
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            rr1["Id"] = 1;
            rr1["Name"] = "A";
            refreshedTable.AddLoadedRow(rr1);

            // Act
            // Merge options are part of the contract surface; using defaults here exercises the standard behavior.
            MergeOptions options = new MergeOptions();
            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Assert
            // Baseline row remains, local Added row remains too
            Assert.Equal(2, currentTable.Rows.Count);

            bool hasId1 = false;
            bool hasId999 = false;

            for (int i = 0; i < currentTable.Rows.Count; i++)
            {
                IDataRow row = currentTable.Rows[i];
                if ((int)row["Id"]! == 1)
                {
                    hasId1 = true;
                }
                if ((int)row["Id"]! == 999)
                {
                    hasId999 = true;
                }
            }

            Assert.True(hasId1);
            Assert.True(hasId999);

            // Refresh should not report the local Added row as deleted
            Assert.Empty(result.DeletedDataRows);
        }
    }
}
