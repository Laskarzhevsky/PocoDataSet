using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataSetExtensions
{
    public partial class ObservableDataSetExtensionsTests
    {
        [Fact]
        public void MergeWithTest_RefreshMode()
        {
            // Arrange
            // 1. Create observable data set with a newly added row (client-side)
            IObservableDataSet currentObservableDataSet = new ObservableDataSet();

            IObservableDataTable departmentObservableDataTable = currentObservableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Sales";
            departmentObservableDataRow.AcceptChanges(); // put row into Unchanged state

            // 2. Create a refreshed data set returned from database / service (non-observable)
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable departmentRefreshedDataTable = refreshedDataSet.AddNewTable("Department");
            departmentRefreshedDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentRefreshedDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow departmentRefreshedDataRow = departmentRefreshedDataTable.AddNewRow();
            departmentRefreshedDataRow["Id"] = 1;
            departmentRefreshedDataRow["Name"] = "Sales and Marketing";

            // Act
            // 3. Merge refreshed values into the current observable data set (Refresh mode)
            IObservableMergeOptions observableMergeOptions = new ObservableMergeOptions();
            observableMergeOptions.MergeMode = MergeMode.RefreshPreservingLocalChanges;

            IObservableDataSetMergeResult mergeResult = currentObservableDataSet.MergeWith(refreshedDataSet, observableMergeOptions);

            // Assert
            // 4. Current observable row now has refreshed values "Sales and Marketing"
            string? name = (string?)departmentObservableDataTable.Rows[0]["Name"];

            Assert.Equal("Sales and Marketing", name);
        }
    }
}
