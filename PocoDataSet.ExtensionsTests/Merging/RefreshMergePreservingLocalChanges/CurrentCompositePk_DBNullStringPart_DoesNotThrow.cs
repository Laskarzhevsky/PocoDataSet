using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Composite primary key null-part policy matrix (POCO). Current behavior (locked by these
    /// tests): - Current rows MAY contain null/DBNull in a composite PK part. - When that happens,
    /// the current row is treated as invalid for correlation and is effectively replaced by
    /// refreshed data. - The merge must not throw.
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
    /// - This file contains exactly one test method: CurrentCompositePk_DBNullStringPart_DoesNotThrow.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void CurrentCompositePk_DBNullStringPart_DoesNotThrow()
        {
            IDataSet current = CreateCompositePkDataSetWithCurrentRow(1, DBNull.Value);
            IDataSet refreshed = CreateCompositePkRefreshedSnapshot(1, "X", "Refreshed");

            MergeOptions options = new MergeOptions();

            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            IDataTable result = current.Tables["T"];
            Assert.Equal(1, result.Rows.Count);

            IDataRow row = result.Rows[0];
            Assert.Equal("Refreshed", row["Name"]);
        }
    }
}
