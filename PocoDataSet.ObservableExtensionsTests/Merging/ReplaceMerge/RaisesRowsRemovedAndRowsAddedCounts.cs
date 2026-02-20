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
    public partial class ReplaceMerge
    {
        [Fact]
        public void RaisesRowsRemovedAndRowsAddedCounts()
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
