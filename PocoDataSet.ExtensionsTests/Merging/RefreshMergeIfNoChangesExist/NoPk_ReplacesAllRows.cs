using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the "no PK" policy for RefreshIfNoChangesExist.
    ///
    /// Why this matters:
    /// - Without a primary key, the merge cannot correlate existing CURRENT rows to REFRESHED rows.
    /// - The mode must therefore choose a deterministic policy (replace all, append, or throw).
    ///
    /// This test locks the CURRENT observed behavior:
    /// - The merge replaces CURRENT rows with REFRESHED rows when no PK exists.
    /// - MergeResult reports the replaced set as Added + Deleted (no correlation possible).
    ///
    /// How it proves the contract:
    /// - Arrange: CURRENT has 2 rows, REFRESHED has 1 different row, and there is no PK defined.
    /// - Act: run DoRefreshMergeIfNoChangesExist.
    /// - Assert: CURRENT ends with the REFRESHED rows only; MergeResult contains Added and Deleted entries.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void NoPk_ReplacesAllRows()
        {
            // Arrange CURRENT without any PK.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            IDataRow c2 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c2["Name"] = "Two";
            t.AddLoadedRow(c2);

            // Arrange REFRESHED without any PK (same schema, different content).
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Name"] = "ServerOnly";
            rt.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: CURRENT rows replaced by REFRESHED rows.
            Assert.Single(t.Rows);
            Assert.Equal("ServerOnly", (string)t.Rows[0]["Name"]!);
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);

            // With no PK, correlation is impossible: expect Added + Deleted reporting.
            Assert.Single(options.DataSetMergeResult.AddedDataRows);
            Assert.Equal(2, options.DataSetMergeResult.DeletedDataRows.Count);
        }
    }
}
