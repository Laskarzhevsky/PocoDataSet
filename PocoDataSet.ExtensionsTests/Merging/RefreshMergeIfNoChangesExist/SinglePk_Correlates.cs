using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the "simple Id PK" correlation path for RefreshIfNoChangesExist.
    ///
    /// Meaning:
    /// - When both CURRENT and REFRESHED tables have a single-column primary key ("Id"),
    ///   refresh merge must correlate rows by Id and update the existing CURRENT row in-place.
    ///
    /// Expected behavior (current observed contract):
    /// - The correlated row's values are updated from REFRESHED.
    /// - The row remains DataRowState.Unchanged because refresh applies server truth as "loaded".
    /// - MergeResult records the row under UpdatedDataRows.
    ///
    /// How it proves the contract:
    /// - Arrange: CURRENT contains Id=1 with Name="One". REFRESHED contains Id=1 with Name changed.
    /// - Act: run DoRefreshMergeIfNoChangesExist.
    /// - Assert: same row (by Id) now has updated Name, remains Unchanged, and MergeResult reports one update.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void SinglePk_Correlates()
        {
            // Arrange: CURRENT has a single PK (Id) and no pending local changes.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            // REFRESHED contains the same Id but with updated server values.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One (refreshed)";
            rt.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();

            // Act: RefreshIfNoChangesExist is allowed because CURRENT has no pending local changes.
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: values updated in-place for Id=1.
            Assert.Equal(1, t.Rows.Count);
            Assert.Equal("One (refreshed)", t.Rows[0]["Name"]);

            // Refresh merge applies server truth as loaded data -> row remains Unchanged.
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);

            // MergeResult reports one update for the correlated row.
            Assert.Single(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Equal(1, (int)options.DataSetMergeResult.UpdatedDataRows[0].DataRow["Id"]!);
        }
    }
}
