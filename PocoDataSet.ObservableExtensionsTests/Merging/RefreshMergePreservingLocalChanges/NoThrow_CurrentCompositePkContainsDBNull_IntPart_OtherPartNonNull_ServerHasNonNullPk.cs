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
        /// Verifies DoesNotThrow WhenCurrentCompositePkContainsDBNull IntPart AndOtherPartNonNull AndRefreshedHasNonNullPk in RefreshMergePreservingLocalChanges merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void NoThrow_CurrentCompositePkContainsDBNull_IntPart_OtherPartNonNull_ServerHasNonNullPk()
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