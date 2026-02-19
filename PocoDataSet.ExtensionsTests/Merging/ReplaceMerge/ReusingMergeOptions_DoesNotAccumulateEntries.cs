using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Replace merge safety contract: reusing the same MergeOptions instance across runs must NOT accumulate result entries.
    ///
    /// Meaning:
    /// - MergeOptions contains a DataSetMergeResult instance (kept stable).
    /// - Each merge should clear previous entries so the result reflects ONLY the latest merge.
    ///
    /// Scenario:
    /// - Run DoReplaceMerge twice with the same MergeOptions instance.
    ///
    /// How the test proves the contract:
    /// - After the first merge, AddedDataRows count == number of snapshot rows.
    /// - After the second merge, AddedDataRows count is still == number of snapshot rows (not doubled).
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void ReusingMergeOptions_DoesNotAccumulateEntries()
        {
            // Arrange
            IDataSet current = new DataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow cur = t.AddNewRow();
            cur["Id"] = 1;
            cur["Name"] = "Old";

            IDataSet refreshed = new DataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r = rt.AddNewRow();
            r["Id"] = 2;
            r["Name"] = "New";

            MergeOptions options = new MergeOptions();
            IDataSetMergeResult stableInstance = options.DataSetMergeResult;

            // Act 1
            current.DoReplaceMerge(refreshed, options);

            // Assert after first run
            Assert.Same(stableInstance, options.DataSetMergeResult);
            Assert.Single(options.DataSetMergeResult.AddedDataRows);

            // Act 2 (same options reused)
            current.DoReplaceMerge(refreshed, options);

            // Assert after second run: still one entry, not accumulated.
            Assert.Same(stableInstance, options.DataSetMergeResult);
            Assert.Single(options.DataSetMergeResult.AddedDataRows);
        }
    }
}
