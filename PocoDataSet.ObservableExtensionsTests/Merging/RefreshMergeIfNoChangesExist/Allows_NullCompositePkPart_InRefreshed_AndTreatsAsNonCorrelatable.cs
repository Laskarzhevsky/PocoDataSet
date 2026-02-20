using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    /// <summary>
    /// Composite primary key matrix (Observable).
    /// NOTE: Based on observed behavior, RefreshIfNoChangesExist does not throw when refreshed composite PK contains null/DBNull,
    /// while RefreshPreservingLocalChanges does throw for null PK parts.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void Allows_NullCompositePkPart_InRefreshed_AndTreatsAsNonCorrelatable()
        {
            IObservableDataSet current = MergeTestingHelpers.CreateCurrentObservableCompositePk(1, "X", "Current");

            IDataSet refreshed = MergeTestingHelpers.CreateRefreshedCompositePk(objectA: 1, objectB: null, name: "Bad");

            ObservableMergeOptions options = new ObservableMergeOptions();

            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            Assert.Single(options.ObservableDataSetMergeResult.AddedObservableDataRows);
            Assert.Empty(options.ObservableDataSetMergeResult.UpdatedObservableDataRows);
            Assert.Single(options.ObservableDataSetMergeResult.DeletedObservableDataRows);

            IObservableDataTable result = current.Tables["T"];
            Assert.Single(result.Rows);

            Assert.Equal(1, (int)result.Rows[0]["A"]!);
            Assert.Null(result.Rows[0]["B"]);
            Assert.Equal("Bad", (string)result.Rows[0]["Name"]!);
        }
    }
}
