using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    /// <summary>
    /// Composite primary key null-part policy matrix (Observable).
    ///
    /// Current behavior (locked by these tests):
    /// - Current observable rows MAY contain null/DBNull in a composite PK part.
    /// - When that happens, the current row is treated as invalid for correlation and is effectively replaced by refreshed data.
    /// - The merge must not throw.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void DoesNotThrow_WhenCurrentCompositePkContainsDBNull_IntPart_AndOtherPartNonNull_AndRefreshedHasNonNullPk()
        {
            IObservableDataSet current = MergeTestingHelpers.CreateCurrentObservableWithCompositePkRow(DBNull.Value, "X");
            IDataSet refreshed = MergeTestingHelpers.CreateRefreshedCompositePkSnapshot(1, "X", "Refreshed");

            ObservableMergeOptions options = new ObservableMergeOptions();

            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            IObservableDataTable result = current.Tables["T"];
            Assert.Equal(1, result.Rows.Count);

            IObservableDataRow row = result.Rows[0];
            Assert.Equal("Refreshed", row["Name"]);
        }
    }
}
