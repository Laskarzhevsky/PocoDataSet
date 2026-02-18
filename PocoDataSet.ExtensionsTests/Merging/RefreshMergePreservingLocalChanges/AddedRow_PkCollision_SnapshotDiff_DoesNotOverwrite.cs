using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Row-state focused variant of the PK-collision conflict scenario.
        ///
        /// Scenario:
        /// - CURRENT has a local Added row (in-memory only).
        /// - REFRESHED snapshot contains a row with the same PK but different non-key values.
        ///
        /// Expected behavior (RefreshPreservingLocalChanges):
        /// - The local Added row is preserved (local wins).
        /// - Snapshot values do not overwrite the local Added row.
        /// - No duplicate "server" row is appended.
        ///
        /// This test exists separately from the correlation test to keep the row-state guarantee obvious.
        /// </summary>
        [Fact]
        public void AddedRow_PkCollision_SnapshotDiff_DoesNotOverwrite()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable ct = current.AddNewTable("T");
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            IDataRow localAdded = ct.AddNewRow();
            localAdded["Id"] = 5;
            localAdded["Name"] = "LOCAL_PAYLOAD";

            // REFRESHED snapshot has the same PK but a different payload.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow snapshotRow = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            snapshotRow["Id"] = 5;
            snapshotRow["Name"] = "SERVER_PAYLOAD";
            rt.AddLoadedRow(snapshotRow);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Assert
            // Still exactly one row with PK=5.
            Assert.Single(ct.Rows);

            // The row is still the local Added row.
            Assert.Equal(DataRowState.Added, ct.Rows[0].DataRowState);

            // Snapshot must not overwrite local payload.
            Assert.Equal("LOCAL_PAYLOAD", (string)ct.Rows[0]["Name"]!);

            // Snapshot must not be appended as a new row.
            Assert.Empty(result.AddedDataRows);
        }
    }
}
