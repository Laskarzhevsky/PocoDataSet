using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    /// <summary>
    /// Additional high-value coverage for the observable merge pipeline after the "no MergeMode / no policies" refactor.
    /// These tests focus on invariants that are easy to regress during future edits.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void ObservableDataSetMergeResultReferenceIsStable_AndCollectsEntries()
        {
            // Arrange
            ObservableDataSet current = MergeTestingHelpers.CreateCurrentObservableDepartmentDataSet();

            // Refreshed adds Id=3, removes Id=2, updates Id=1
            IDataSet refreshed = MergeTestingHelpers.CreateRefreshedDepartmentDataSet();

            ObservableMergeOptions options = new ObservableMergeOptions();
            IObservableDataSetMergeResult initialResult = options.ObservableDataSetMergeResult;

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert: same instance
            Assert.Same(initialResult, options.ObservableDataSetMergeResult);

            // And it collected something meaningful (added + deleted + updated should all be possible here)
            Assert.True(
                options.ObservableDataSetMergeResult.AddedObservableDataRows.Count > 0
                || options.ObservableDataSetMergeResult.DeletedObservableDataRows.Count > 0
                || options.ObservableDataSetMergeResult.UpdatedObservableDataRows.Count > 0);
        }
    }
}
