using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the duplicate-PK policy for RefreshIfNoChangesExist when the REFRESHED snapshot
    /// contains multiple rows with the same primary key value.
    ///
    /// Why this matters:
    /// - Duplicate PKs in server snapshots are data-quality bugs, but they do happen.
    /// - Merge code must behave deterministically to avoid non-reproducible client state.
    ///
    /// This test locks the CURRENT observed behavior:
    /// - The merge does not throw.
    /// - The first REFRESHED row for a given PK wins (its values become the final values).
    ///
    /// How it proves the contract:
    /// - Arrange: CURRENT has Id=1 Name="One". REFRESHED has TWO Id=1 rows with different Names.
    /// - Act: run DoRefreshMergeIfNoChangesExist.
    /// - Assert: CURRENT ends with the first value and MergeResult reports an update for Id=1.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void DuplicatePk_Refreshed_FirstWins()
        {
            // Arrange CURRENT.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            // Arrange REFRESHED with duplicate PK rows (Id=1 appears twice).
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One (first)";
            rt.AddLoadedRow(r1);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r2["Id"] = 1;
            r2["Name"] = "One (last)";
            rt.AddLoadedRow(r2);

            MergeOptions options = new MergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: deterministic policy (first wins).
            Assert.Single(t.Rows);
            Assert.Equal("One (first)", (string)t.Rows[0]["Name"]!);

            // The refreshed value is treated as loaded data.
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);

            // MergeResult must report an update for the correlated row.
            Assert.Single(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Equal(1, (int)options.DataSetMergeResult.UpdatedDataRows[0].DataRow["Id"]!);
        }
    }
}
