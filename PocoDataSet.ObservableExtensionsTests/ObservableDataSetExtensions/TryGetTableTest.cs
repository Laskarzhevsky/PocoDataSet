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
        public void TryGetTableTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Reception";


            // Act
            // 3. Safely access an optional observable table
            observableDataSet.TryGetTable("Department", out IObservableDataTable? retreivedDepartmentObservableDataTable);
            observableDataSet.TryGetTable("Employee", out IObservableDataTable? retreivedEmployeeObservableDataTable);

            // Assert
            Assert.NotNull(retreivedDepartmentObservableDataTable);
            Assert.Null(retreivedEmployeeObservableDataTable);
        }
    }
}
