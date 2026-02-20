using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies Throws WhenRefreshedCompositePkContainsNullPart in RefreshMergePreservingLocalChanges merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void Throws_ServerCompositePkContainsNullPart()
        {
            IObservableDataSet current = MergeTestingHelpers.CreateCurrentObservableCompositePk(1, "X", "Current");

            IDataSet refreshed = MergeTestingHelpers.CreateRefreshedCompositePk(objectA: 1, objectB: null, name: "Bad");

            ObservableMergeOptions options = new ObservableMergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergePreservingLocalChanges(refreshed, options));
        }
    }
}