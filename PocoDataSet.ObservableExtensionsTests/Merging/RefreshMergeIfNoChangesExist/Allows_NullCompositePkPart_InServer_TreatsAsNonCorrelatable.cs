using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class RefreshMergeIfNoChangesExist
    {
        /// <summary>
        /// Verifies Allows NullCompositePkPart InRefreshed AndTreatsAsNonCorrelatable in RefreshMergeIfNoChangesExist merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void Allows_NullCompositePkPart_InServer_TreatsAsNonCorrelatable()
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