using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the "snapshot has extra row" outcome for RefreshIfNoChangesExist.
    ///
    /// Meaning:
    /// - If REFRESHED contains a PK row not present in CURRENT, refresh merge must add it.
    ///
    /// Expected behavior (current observed contract):
    /// - The added row appears in CURRENT after the merge.
    /// - The added row is treated as loaded data and ends as Unchanged.
    /// - MergeResult records the row under AddedDataRows.
    ///
    /// How it proves the contract:
    /// - Arrange: CURRENT has Id=1. REFRESHED has Id=1 and Id=2.
    /// - Act: run DoRefreshMergeIfNoChangesExist.
    /// - Assert: CURRENT now contains Id=2 and MergeResult has one Added entry for Id=2.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void AddsRow_WhenSnapshotHasExtra()
        {
            // Arrange CURRENT: one loaded row.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            // Arrange REFRESHED: includes an extra row Id=2.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One";
            rt.AddLoadedRow(r1);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r2["Id"] = 2;
            r2["Name"] = "Two (new)";
            rt.AddLoadedRow(r2);

            MergeOptions options = new MergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: Id=2 was added.
            Assert.Equal(2, t.Rows.Count);
            Assert.True(RowExistsById(t, 2));

                        // Find the newly added row (Id=2) without relying on helpers, to keep this test fully local.
            IDataRow? added = null;
            for (int i = 0; i < t.Rows.Count; i++)
            {
                if ((int)t.Rows[i]["Id"]! == 2)
                {
                    added = t.Rows[i];
                    break;
                }
            }

            Assert.NotNull(added);
            Assert.Equal("Two (new)", (string)added!["Name"]!);

            // Added rows from refresh are "loaded" server truth.
            Assert.Equal(DataRowState.Unchanged, added.DataRowState);

            // MergeResult records the addition.
            Assert.Single(options.DataSetMergeResult.AddedDataRows);
            Assert.Equal(2, (int)options.DataSetMergeResult.AddedDataRows[0].DataRow["Id"]!);
        }
    }
}
