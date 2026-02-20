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
        /// Verifies DoesNotRaiseDataFieldValueChanged WhenRefreshedValuesAreIdentical in RefreshMergePreservingLocalChanges merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void NoRaiseDataFieldValueChanged_ServerValuesAreIdentical()
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