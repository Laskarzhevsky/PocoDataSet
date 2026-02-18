using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the "snapshot is missing a current row" outcome for RefreshIfNoChangesExist.
    ///
    /// Meaning:
    /// - If CURRENT has a PK row that does not appear in REFRESHED, refresh merge must remove it.
    ///
    /// Expected behavior (current observed contract):
    /// - The missing row is removed from CURRENT's table.
    /// - MergeResult records the missing row under DeletedDataRows.
    ///
    /// How it proves the contract:
    /// - Arrange: CURRENT has Id=1 and Id=2. REFRESHED only has Id=1.
    /// - Act: run DoRefreshMergeIfNoChangesExist.
    /// - Assert: Id=2 no longer exists and MergeResult contains one Deleted entry for Id=2.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void DeletesRow_WhenSnapshotMissing()
        {
            // Arrange CURRENT: two loaded rows.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            IDataRow c2 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c2["Id"] = 2;
            c2["Name"] = "Two";
            t.AddLoadedRow(c2);

            // Arrange REFRESHED: only Id=1, so Id=2 should be removed.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One";
            rt.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: Id=2 removed.
            Assert.Equal(1, t.Rows.Count);
            Assert.False(RowExistsById(t, 2));

            // MergeResult records the deletion by PK.
            Assert.Single(options.DataSetMergeResult.DeletedDataRows);
            Assert.Equal(2, (int)options.DataSetMergeResult.DeletedDataRows[0].DataRow["Id"]!);
        }
    }
}
