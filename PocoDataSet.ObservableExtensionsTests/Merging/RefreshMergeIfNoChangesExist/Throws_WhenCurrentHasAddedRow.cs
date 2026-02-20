using System;

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
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void Throws_WhenCurrentHasAddedRow()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable currentDept = current.AddNewTable("Department");
            currentDept.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDept.AddColumn("Name", DataTypeNames.STRING);

            // Added row (not accepted)
            IObservableDataRow added = currentDept.AddNewRow();
            added["Id"] = 1;
            added["Name"] = "Sales";

            IDataSet refreshed = MergeTestingHelpers.CreateDepartmentRefreshedSnapshot(id1Name: "Sales");

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act / Assert
            Assert.Throws<InvalidOperationException>(delegate
            {
                current.DoRefreshMergeIfNoChangesExist(refreshed, options);
            });
        }
    }
}
