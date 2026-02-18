using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies that **RefreshPreservingLocalChanges preserves a locally Modified row** even when the refreshed
        /// snapshot does not contain that row (server-side deletion or filtering).
        ///
        /// Contract intent:
        /// - "Preserve local changes" means local modifications must not be discarded just because the server snapshot
        ///   is missing the row.
        /// - Only Unchanged rows are eligible to be removed when missing from the snapshot (you already lock that).
        ///
        /// How the test proves the contract:
        /// 1) CURRENT: create a loaded row (Id=1), then change a field so its state becomes Modified.
        /// 2) REFRESHED: provide an empty snapshot (row missing).
        /// 3) Act: merge in RefreshPreservingLocalChanges mode.
        /// 4) Assert: row still exists and remains Modified, with the local value preserved.
        /// </summary>
        [Fact]
        public void LocalModifiedRow_Preserved_WhenSnapshotDeletes()
        {
            // Arrange (CURRENT)
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
            r["Id"] = 1;
            r["Name"] = "A";
            t.AddLoadedRow(r);

            // Local edit
            t.Rows[0]["Name"] = "A (local edit)";
            Assert.Equal(DataRowState.Modified, t.Rows[0].DataRowState);

            // Arrange (REFRESHED) - snapshot missing the row entirely
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("Department");
            rt.AddColumn("Id", DataTypeNames.INT32, true);
            rt.AddColumn("Name", DataTypeNames.STRING);
            // no rows

            // Act
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            // Modified row must be preserved (not removed) and its local value must remain.
            Assert.Equal(1, t.Rows.Count);
            Assert.Equal(DataRowState.Modified, t.Rows[0].DataRowState);
            Assert.Equal("A (local edit)", t.Rows[0]["Name"]);
        }
    }
}
