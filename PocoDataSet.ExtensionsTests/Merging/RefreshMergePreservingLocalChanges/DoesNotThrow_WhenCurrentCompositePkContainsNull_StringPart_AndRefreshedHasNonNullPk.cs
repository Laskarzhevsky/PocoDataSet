using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Composite primary key null-part policy matrix (POCO).
    ///
    /// Current behavior (locked by these tests):
    /// - Current rows MAY contain null/DBNull in a composite PK part.
    /// - When that happens, the current row is treated as invalid for correlation and is effectively replaced by refreshed data.
    /// - The merge must not throw.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void DoesNotThrow_WhenCurrentCompositePkContainsNull_StringPart_AndRefreshedHasNonNullPk()
        {
            IDataSet current = CreateCompositePkDataSetWithCurrentRow(1, null);
            IDataSet refreshed = CreateCompositePkRefreshedSnapshot(1, "X", "Refreshed");

            MergeOptions options = new MergeOptions();

            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            IDataTable result = current.Tables["T"];
            Assert.Equal(1, result.Rows.Count);

            IDataRow row = result.Rows[0];
            Assert.Equal("Refreshed", row["Name"]);
        }
    }
}
