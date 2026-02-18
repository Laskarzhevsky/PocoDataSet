using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Composite primary key matrix (POCO). NOTE: Your current merge contract differs by mode: -
    /// RefreshPreservingLocalChanges rejects refreshed composite PK rows containing null parts
    /// (throws). - RefreshIfNoChangesExist currently allows refreshed composite PK rows containing
    /// null parts and treats them as non-correlatable. These tests lock the CURRENT observed
    /// behavior to prevent future regressions.
    ///
    /// Scenario:
    /// - Build a CURRENT DataSet (the client-side truth before the merge).
    /// - Build a REFRESHED/CHANGESET DataSet (the server-side snapshot or post-save response).
    /// - Execute RefreshPreservingLocalChanges merge (refresh unchanged rows, preserve local changes).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
    /// Notes:
    /// - This file contains exactly one test method: Correlates_WhenAllCompositePkPartsMatch.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void Correlates_WhenAllCompositePkPartsMatch()
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

            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            Assert.Single(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Single(options.DataSetMergeResult.DeletedDataRows);
            Assert.Single(options.DataSetMergeResult.AddedDataRows);

            IDataTable result = current.Tables["T"];
            Assert.Equal(2, result.Rows.Count);

            Assert.True(ContainsCompositePk(result, 1, "X"));
            Assert.True(ContainsCompositePk(result, 1, "Z"));
            Assert.False(ContainsCompositePk(result, 1, "Y"));

            IDataRow rowX = FindByCompositePk(result, 1, "X");
            Assert.Equal("One-X (updated)", (string)rowX["Name"]!);
        }
    }
}
