using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class ReplaceMerge
    {
        /// <summary>
        /// Verifies RaisesRowsRemovedAndRowsAddedCounts in ReplaceMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void RaisesRowsRemoved_RowsAddedCounts()
        {
            // Arrange: current observable data set with 2 rows
            ObservableDataSet current = MergeTestingHelpers.CreateCurrentObservableDepartmentDataSet();
            IObservableDataView? view = current.GetObservableDataView("Department", null, false, "Id ASC", "ScreenReplace");
            Assert.NotNull(view);

            RowsChangedCounter removedCounter = new RowsChangedCounter();
            RowsChangedCounter addedCounter = new RowsChangedCounter();

            view.RowsRemoved += removedCounter.Handler;
            view.RowsAdded += addedCounter.Handler;

            // Refreshed has 1 row (Id=1)
            IDataSet refreshed = MergeTestingHelpers.CreateDepartmentRefreshedSnapshot(id1Name: "SalesRefreshed");

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            Assert.Equal(1, view.Rows.Count);
            Assert.Equal(1, view.Rows[0].GetDataFieldValue<int>("Id"));

            // 2 removed, 1 added
            Assert.Equal(2, removedCounter.Count);
            Assert.Equal(1, addedCounter.Count);
        }
    }
}