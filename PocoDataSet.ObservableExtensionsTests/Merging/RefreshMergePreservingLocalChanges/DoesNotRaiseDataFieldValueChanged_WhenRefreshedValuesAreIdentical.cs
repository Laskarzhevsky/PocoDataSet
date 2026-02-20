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
        public void DoesNotRaiseDataFieldValueChanged_WhenRefreshedValuesAreIdentical()
        {
            // Arrange
            ObservableDataSet current = MergeTestingHelpers.CreateCurrentObservableDepartmentDataSet();
            IObservableDataTable currentTable = current.Tables["Department"];

            IObservableDataRow row1 = MergeTestingHelpers.FindById(currentTable, 1);
            DataFieldValueChangedCounter counter = new DataFieldValueChangedCounter();
            row1.DataFieldValueChanged += counter.Handler;

            // Refreshed snapshot has the exact same values as current for Id=1.
            IDataSet refreshed = MergeTestingHelpers.CreateDepartmentRefreshedSnapshot("Sales");

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Equal(0, counter.Count);
        }
    }
}
