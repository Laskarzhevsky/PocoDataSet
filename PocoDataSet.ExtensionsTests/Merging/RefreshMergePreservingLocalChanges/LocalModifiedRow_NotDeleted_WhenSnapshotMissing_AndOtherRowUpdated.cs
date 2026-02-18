using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Variant coverage that guards against "early exit" bugs in RefreshPreservingLocalChanges.
        ///
        /// Scenario:
        /// - CURRENT has two rows:
        ///   (1) Id=1 is locally Modified.
        ///   (2) Id=2 is Unchanged.
        /// - REFRESHED snapshot:
        ///   - Omits Id=1 (snapshot deletion / missing row).
        ///   - Includes Id=2 with an updated value.
        ///
        /// Expected behavior (local wins):
        /// - The locally Modified row (Id=1) must NOT be deleted just because it is missing from the snapshot.
        /// - The other row (Id=2) must still be refreshed/updated.
        /// - MergeResult records the update for Id=2 and does not record a delete for Id=1.
        ///
        /// Why this test exists:
        /// Some merge implementations accidentally stop processing after encountering a "missing" row.
        /// This test ensures the merge continues and applies safe updates while preserving local edits.
        /// </summary>
        [Fact]
        public void LocalModifiedRow_NotDeleted_WhenSnapshotMissing_AndOtherRowUpdated()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable ct = current.AddNewTable("T");
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            // Id=1 is loaded then locally modified.
            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(ct.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            ct.AddLoadedRow(c1);

            // Id=2 is loaded unchanged.
            IDataRow c2 = DataRowExtensions.CreateRowFromColumns(ct.Columns);
            c2["Id"] = 2;
            c2["Name"] = "Two";
            ct.AddLoadedRow(c2);

            // Create local modification on Id=1.
            c1["Name"] = "One_LOCAL_EDIT";
            Assert.Equal(DataRowState.Modified, c1.DataRowState);
            Assert.Equal(DataRowState.Unchanged, c2.DataRowState);

            // REFRESHED snapshot omits Id=1 and updates Id=2.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r2["Id"] = 2;
            r2["Name"] = "Two_SERVER";
            rt.AddLoadedRow(r2);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Assert
            // Id=1 is preserved (not deleted), with its local modification intact.
            Assert.Equal(2, ct.Rows.Count);

            IDataRow? found1 = null;
            IDataRow? found2 = null;
            for (int i = 0; i < ct.Rows.Count; i++)
            {
                IDataRow row = ct.Rows[i];
                if ((int)row["Id"]! == 1)
                {
                    found1 = row;
                }
                if ((int)row["Id"]! == 2)
                {
                    found2 = row;
                }
            }

            Assert.NotNull(found1);
            Assert.NotNull(found2);

            Assert.Equal("One_LOCAL_EDIT", (string)found1!["Name"]!);
            Assert.Equal(DataRowState.Modified, found1.DataRowState);

            // Id=2 must still be refreshed.
            Assert.Equal("Two_SERVER", (string)found2!["Name"]!);
            Assert.Equal(DataRowState.Unchanged, found2.DataRowState);

            // MergeResult: update recorded for Id=2, no delete recorded for Id=1.
            Assert.Single(result.UpdatedDataRows);
            Assert.Empty(result.DeletedDataRows);
        }
    }
}
