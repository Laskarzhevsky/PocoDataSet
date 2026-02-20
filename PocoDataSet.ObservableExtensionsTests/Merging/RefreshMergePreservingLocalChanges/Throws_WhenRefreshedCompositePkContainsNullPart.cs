using System;

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
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void Throws_WhenRefreshedCompositePkContainsNullPart()
        {
            IObservableDataSet current = MergeTestingHelpers.CreateCurrentObservableCompositePk(1, "X", "Current");

            IDataSet refreshed = MergeTestingHelpers.CreateRefreshedCompositePk(objectA: 1, objectB: null, name: "Bad");

            ObservableMergeOptions options = new ObservableMergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergePreservingLocalChanges(refreshed, options));
        }
    }
}
