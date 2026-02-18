using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies the correlation/conflict policy for RefreshPreservingLocalChanges when a LOCAL Added row
        /// collides with an existing PK in the refreshed snapshot and the snapshot has different non-key values.
        ///
        /// Contract this test locks:
        /// - RefreshPreservingLocalChanges must preserve local edits.
        /// - A locally Added row must NOT be overwritten by snapshot values, even when the snapshot contains
        ///   the same PK row (payload conflict).
        /// - The local row remains Added and retains its local non-key values.
        ///
        /// Why this matters:
        /// This prevents "silent overwrite" bugs where a server snapshot could accidentally replace in-progress
        /// local edits before the row is persisted.
        /// </summary>
        [Fact]
        public void LocalAddedRow_PkCollision_SnapshotDiff_LocalWins()
        {
            // Arrange
            // CURRENT contains a locally Added row.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable ct = current.AddNewTable("T");
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            IDataRow localAdded = ct.AddNewRow();
            localAdded["Id"] = 10;
            localAdded["Name"] = "LOCAL";

            Assert.Equal(DataRowState.Added, localAdded.DataRowState);

            // REFRESHED snapshot contains the same PK, but with different payload.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow snapshotRow = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            snapshotRow["Id"] = 10;
            snapshotRow["Name"] = "SERVER";
            rt.AddLoadedRow(snapshotRow);

            // Act
            // Execute RefreshPreservingLocalChanges: it should prefer local changes over snapshot values.
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Assert
            // There is still exactly one row with that PK.
            Assert.Single(ct.Rows);

            // The local Added row must remain Added.
            Assert.Equal(DataRowState.Added, ct.Rows[0].DataRowState);

            // Local values must win over the snapshot.
            Assert.Equal("LOCAL", (string)ct.Rows[0]["Name"]!);

            // This operation must not delete or update the local Added row.
            Assert.Empty(result.DeletedDataRows);
            Assert.Empty(result.UpdatedDataRows);
        }
    }
}
