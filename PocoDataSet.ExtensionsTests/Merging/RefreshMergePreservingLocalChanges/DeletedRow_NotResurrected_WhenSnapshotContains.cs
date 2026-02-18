using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies that **RefreshPreservingLocalChanges does not resurrect a locally Deleted row** even when the
        /// refreshed snapshot still contains that same row (same PK).
        ///
        /// Why this matters:
        /// - A server snapshot can legitimately contain a row that the client has decided to delete (race conditions,
        ///   lagging replication, stale caches, etc.).
        /// - In "preserve local changes" mode, local deletes are authoritative and must win over the snapshot.
        ///
        /// How the test proves the contract:
        /// 1) Arrange: CURRENT has a single loaded row (Id=1), then we delete it locally (state becomes Deleted).
        /// 2) Arrange: REFRESHED snapshot contains Id=1 as if the server still sees it.
        /// 3) Act: run RefreshPreservingLocalChanges merge.
        /// 4) Assert: CURRENT still has exactly one row and it is still Deleted (not resurrected / not overwritten).
        /// </summary>
        [Fact]
        public void DeletedRow_NotResurrected_WhenSnapshotContains()
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

            // Delete locally: this is the key "preserve local changes" signal we want to protect.
            t.Rows[0].Delete();
            Assert.Equal(DataRowState.Deleted, t.Rows[0].DataRowState);

            // Arrange (REFRESHED) - server still includes the row.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("Department");
            rt.AddColumn("Id", DataTypeNames.INT32, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            rr["Id"] = 1;
            rr["Name"] = "A (server)";
            rt.AddLoadedRow(rr);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            // The local delete must win; the row must not be resurrected.
            Assert.Equal(1, t.Rows.Count);
            Assert.Equal(DataRowState.Deleted, t.Rows[0].DataRowState);

            // And the merge result must not claim the row was "updated" (it was deliberately deleted locally).
            Assert.Empty(options.DataSetMergeResult.UpdatedDataRows);
        }
    }
}
