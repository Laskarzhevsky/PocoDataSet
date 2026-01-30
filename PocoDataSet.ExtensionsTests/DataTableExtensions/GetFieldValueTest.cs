using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void GetFieldValueTest()
        {
            // Arrange
            // 1. Create data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Add Employee data table to data set
            IDataTable employeeDataTable = dataSet.AddNewTable("Employee");
            employeeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employeeDataTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeDataTable.AddColumn("LastName", DataTypeNames.STRING);

            // 3. Add several rows to Employee data table
            IDataRow employeeDataRow1 = employeeDataTable.AddNewRow();
            employeeDataRow1["Id"] = 1;
            employeeDataRow1["FirstName"] = "John";
            employeeDataRow1["LastName"] = "Doe";

            IDataRow employeeDataRow2 = employeeDataTable.AddNewRow();
            employeeDataRow2["Id"] = 2;
            employeeDataRow2["FirstName"] = "Sara";
            employeeDataRow2["LastName"] = "Gor";

            // Act
            // 4. Call GetFieldValue method to get a value from the FirstName field of the first row, expected "John"
            string? firstName = employeeDataTable.GetFieldValue<string>(0, "firstname");

            // Assert
            Assert.Equal("John", firstName);
        }
    }
}
