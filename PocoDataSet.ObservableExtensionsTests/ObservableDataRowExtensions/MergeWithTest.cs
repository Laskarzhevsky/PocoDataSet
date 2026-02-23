using System;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataRowExtensions
{
    public partial class ObservableDataRowExtensionsTests
    {
        [Fact]
        public void MergeWith_RowInModifiedState()
        {
            // 1. Create a new observable data set to simulate UI bound data source
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create observable table and row
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Sales";

            // 3. Accept the row changes and modify its data to put row into Modified state
            departmentObservableDataRow.AcceptChanges();
            departmentObservableDataRow["Name"] = "Reception";

            // 4. Create a new data set simulating refreshed data came form database
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();

            IDataTable refreshedDepartmentDataTable = refreshedDataSet.AddNewTable("Department");
            refreshedDepartmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow departmentRefreshedDataRow = refreshedDepartmentDataTable.AddNewRow();
            departmentRefreshedDataRow["Id"] = 1;
            departmentRefreshedDataRow["Name"] = "Financial";
            departmentRefreshedDataRow.AcceptChanges();

            // Act
            // 5. Call MergeWith method and observe that merge is not done because the observable row in Modified state
            observableDataSet.DoRefreshMergePreservingLocalChanges(refreshedDataSet, new ObservableMergeOptions());

            // Assert
            Assert.Equal(departmentObservableDataRow.DataRowState.ToString(), DataRowState.Modified.ToString());
            Assert.Equal("Reception", departmentObservableDataRow["Name"]);
        }

        [Fact]
        public void MergeWith_RowInDeletedState()
        {
            // Arrange
            // 1. Create a new observable data set to simulate UI bound data source
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create observable table and row
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Sales";

            // 3. Accept the row changes and and call Delete method to put row into Modified state
            departmentObservableDataRow.AcceptChanges();
            departmentObservableDataRow.Delete();

            // 4. Create a new data set simulating refreshed data came form database
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();

            IDataTable refreshedDepartmentDataTable = refreshedDataSet.AddNewTable("Department");
            refreshedDepartmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow departmentRefreshedDataRow = refreshedDepartmentDataTable.AddNewRow();
            departmentRefreshedDataRow["Id"] = 1;
            departmentRefreshedDataRow["Name"] = "Financial";
            departmentRefreshedDataRow.AcceptChanges();

            // Act
            // 5. Call MergeWith method and observe that merge is not done because the observable row in Deleted state
            observableDataSet.DoRefreshMergePreservingLocalChanges(refreshedDataSet, new ObservableMergeOptions());

            // Assert
            Assert.Equal(DataRowState.Deleted.ToString(), departmentObservableDataRow.DataRowState.ToString());
            Assert.Equal("Sales", departmentObservableDataRow["Name"]);
        }

        [Fact]
        public void MergeWith_RowInUnchangedState()
        {
            // Arrange
            // 1. Create a new observable data set to simulate UI bound data source
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create observable table and row
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Sales";

            // 3. Accept observable row changes to put row into Unchanged state
            departmentObservableDataRow.AcceptChanges();

            // 4. Create a new data set simulating refreshed data came form database
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();

            IDataTable refreshedDepartmentDataTable = refreshedDataSet.AddNewTable("Department");
            refreshedDepartmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow departmentRefreshedDataRow = refreshedDepartmentDataTable.AddNewRow();
            departmentRefreshedDataRow["Id"] = 1;
            departmentRefreshedDataRow["Name"] = "Financial";
            departmentRefreshedDataRow.AcceptChanges();

            // Act
            // 5. Call MergeWith method and observe that merge is completed because the observable row was in Unchanged state
            observableDataSet.DoRefreshMergePreservingLocalChanges(refreshedDataSet, new ObservableMergeOptions());

            // Assert
            Assert.Equal(DataRowState.Unchanged.ToString(), departmentObservableDataRow.DataRowState.ToString());
            Assert.Equal("Financial", departmentObservableDataRow["Name"]);
        }
    }
}
