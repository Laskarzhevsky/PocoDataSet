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
        public void ReloadFromTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table with rows in different states
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            // Row in Added state
            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Reception";

            // Row in Deleted state
            departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 2;
            departmentObservableDataRow["Name"] = "Emergency";
            departmentObservableDataRow.AcceptChanges();
            departmentObservableDataRow.Delete();

            // Row in Modified state
            departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 3;
            departmentObservableDataRow["Name"] = "Emergency";
            departmentObservableDataRow.AcceptChanges();
            departmentObservableDataRow["Name"] = "Finance";

            // Row in Unchanged state
            departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 4;
            departmentObservableDataRow["Name"] = "Customer Service";
            departmentObservableDataRow.AcceptChanges();

            // 3. Create refreshed data set
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartmentDataTable = refreshedDataSet.AddNewTable("Department");
            refreshedDepartmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedDepartmentDataRow = refreshedDepartmentDataTable.AddNewRow();
            refreshedDepartmentDataRow["Id"] = 1;
            refreshedDepartmentDataRow["Name"] = "Sales";

            // Act
            // 4. Call ReloadFrom method to reload observable data set from refreshed data set
            observableDataSet.ReloadFrom(refreshedDataSet);

            // Assert
            IObservableDataTable departmentReloadedObservableDataTable = observableDataSet.Tables["Department"];
            IObservableDataRow departmentReloadedObservableDataRow =  departmentReloadedObservableDataTable.Rows[0];
            DataRowState dataRowState = departmentReloadedObservableDataRow.DataRowState;
            int id = departmentReloadedObservableDataRow.GetDataFieldValue<int>("Id");
            string? name = departmentReloadedObservableDataRow.GetDataFieldValue<string>("Name");

            Assert.Single(departmentReloadedObservableDataTable.Rows);

            Assert.Equal(1, departmentReloadedObservableDataRow["Id"]);
            Assert.Equal("Sales", departmentReloadedObservableDataRow["Name"]);
            Assert.Equal(DataRowState.Unchanged, departmentReloadedObservableDataRow.DataRowState);
        }
    }
}
