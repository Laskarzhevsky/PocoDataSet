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
    /// - Execute RefreshIfNoChangesExist merge (throws if current has pending changes).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
    /// Notes:
    /// - This file contains exactly one test method: NullCompositePkPart_TreatedAsNonCorrelatable.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void NullCompositePkPart_TreatedAsNonCorrelatable()
        {
            // Arrange: current has a valid composite PK row (1, X)
            IDataSet current = CreateCurrentCompositePk(1, "X", "Current");

            // Refreshed uses (1, null) -> cannot correlate to (1, X) on full composite PK
            IDataSet refreshed = CreateRefreshedCompositePk(objectA: 1, objectB: null, name: "Bad");

            MergeOptions options = new MergeOptions();

            // Act (should NOT throw)
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: old row deleted, new row added, no update
            Assert.Single(options.DataSetMergeResult.AddedDataRows);
            Assert.Empty(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Single(options.DataSetMergeResult.DeletedDataRows);

            IDataTable result = current.Tables["T"];
            Assert.Single(result.Rows);

            Assert.Equal(1, (int)result.Rows[0]["A"]!);
            Assert.Null(result.Rows[0]["B"]);
            Assert.Equal("Bad", (string)result.Rows[0]["Name"]!);
        }
    }
}
