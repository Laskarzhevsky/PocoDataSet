using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Composite primary key matrix (POCO).
    /// NOTE: Your current merge contract differs by mode:
    /// - RefreshPreservingLocalChanges rejects refreshed composite PK rows containing null parts (throws).
    /// - RefreshIfNoChangesExist currently allows refreshed composite PK rows containing null parts and treats them as non-correlatable.
    ///
    /// These tests lock the CURRENT observed behavior to prevent future regressions.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void CorrelatesOnlyWhenAllCompositePkPartsMatch()
        {
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(current);

            AddLoadedRow(t, 1, "X", "One-X");
            AddLoadedRow(t, 1, "Y", "One-Y");

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = AddCompositePkTable(refreshed);

            AddLoadedRow(rt, 1, "X", "One-X (updated)");
            AddLoadedRow(rt, 1, "Z", "One-Z");

            MergeOptions options = new MergeOptions();

            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            Assert.Single(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Single(options.DataSetMergeResult.DeletedDataRows);
            Assert.Single(options.DataSetMergeResult.AddedDataRows);

            IDataTable result = current.Tables["T"];
            Assert.Equal(2, result.Rows.Count);

            Assert.True(ContainsCompositePk(result, 1, "X"));
            Assert.True(ContainsCompositePk(result, 1, "Z"));
            Assert.False(ContainsCompositePk(result, 1, "Y"));
        }
    }
}
