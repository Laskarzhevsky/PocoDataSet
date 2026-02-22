using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void AddRelationTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create parent Department data table
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // 3. Create child Employee data table with a foreign key column referencing the parent Department table
            IDataTable employeeDataTable = dataSet.AddNewTable("Employee");
            employeeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employeeDataTable.AddColumn("DepartmentId", DataTypeNames.INT32, true, false, true);

            // 4. Add a relation between the Department and Employee tables
            List<string> parentColumnNames = new List<string>();
            parentColumnNames.Add("Id");

            List<string> childColumnNames = new List<string>();
            childColumnNames.Add("DepartmentId");

            dataSet.AddRelation("DepartmentEmployees", "Department", parentColumnNames, "Employee", childColumnNames);

            // Act
            // 5. Call ContainsRelation to verify that the relation was added successfully
            bool departmentEmployeesRelationExists = dataSet.ContainsRelation("DepartmentEmployees"); // true

            // Assert
            Assert.True(departmentEmployeesRelationExists);
        }
    }
}
