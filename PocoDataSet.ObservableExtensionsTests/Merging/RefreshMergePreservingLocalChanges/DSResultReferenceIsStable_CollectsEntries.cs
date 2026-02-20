using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies ObservableDataSetMergeResultReferenceIsStable AndCollectsEntries in RefreshMergePreservingLocalChanges merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void DSResultReferenceIsStable_CollectsEntries()
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