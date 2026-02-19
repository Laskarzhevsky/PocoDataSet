using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Replace merge row-state + result accounting contract (as implemented today):
    /// - Replace clears CURRENT rows and adds snapshot rows via AddLoadedRow (rows become Unchanged).
    /// - MergeResult tracks these as AddedDataRows (because they are newly created instances in CURRENT).
    /// - Replace does not record Deleted/Updated entries for removed rows.
    ///
    /// Scenario:
    /// - CURRENT has 2 rows.
    /// - REFRESHED has 1 row.
    ///
    /// How the test proves the contract:
    /// - After DoReplaceMerge:
    ///   - CURRENT has exactly 1 row with state Unchanged.
    ///   - MergeResult.AddedDataRows has exactly 1 entry for that row.
    ///   - MergeResult.DeletedDataRows and UpdatedDataRows are empty.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void ResultEntries_Match_FinalState()
        {
            // Arrange
            IDataSet current = new DataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow a = t.AddNewRow();
            a["Id"] = 1;
            a["Name"] = "A";

            IDataRow b = t.AddNewRow();
            b["Id"] = 2;
            b["Name"] = "B";

            IDataSet refreshed = new DataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r = rt.AddNewRow();
            r["Id"] = 10;
            r["Name"] = "Server";

            MergeOptions options = new MergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert: data
            Assert.Single(t.Rows);
            Assert.Equal(10, (int)t.Rows[0]["Id"]!);
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);

            // Assert: merge result accounting
            IDataSetMergeResult result = options.DataSetMergeResult;

            Assert.Single(result.AddedDataRows);
            Assert.Empty(result.DeletedDataRows);
            Assert.Empty(result.UpdatedDataRows);

            Assert.Equal("T", result.AddedDataRows[0].TableName);
            Assert.Same(t.Rows[0], result.AddedDataRows[0].DataRow);
        }
    }
}
