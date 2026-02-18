using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Composite primary key matrix (POCO).
    /// NOTE: Your current merge contract differs by mode:
    /// - RefreshPreservingLocalChanges rejects refreshed composite PK rows containing null parts (throws).
    /// - RefreshIfNoChangesExist currently allows refreshed composite PK rows containing null parts and treats them as non-correlatable.
    ///
    /// These tests lock the CURRENT observed behavior to prevent future regressions.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void Throws_WhenRefreshedCompositePkContainsNullPart()
        {
            IDataSet current = CreateCurrentCompositePk(1, "X", "Current");

            IDataSet refreshed = CreateRefreshedCompositePk(objectA: 1, objectB: null, name: "Bad");

            MergeOptions options = new MergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergePreservingLocalChanges(refreshed, options));
        }
    }
}
