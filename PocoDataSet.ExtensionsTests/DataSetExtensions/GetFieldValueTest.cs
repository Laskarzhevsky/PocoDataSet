using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void GetFieldValueTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create a table and add a row
            IDataTable employeeDataTable = dataSet.AddNewTable("Employee");
            employeeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employeeDataTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeDataTable.AddColumn("LastName", DataTypeNames.STRING);

            IDataRow employeeDataRow = employeeDataTable.AddNewRow();
            employeeDataRow["Id"] = 1;
            employeeDataRow["FirstName"] = "Sara";
            employeeDataRow["LastName"] = "Gor";

            // Act
            // 3. Read a value using GetFieldValue
            string? firstName = dataSet.GetFieldValue<string>("Employee", 0, "FirstName");

            // Assert
            Assert.Equal("Sara", firstName);
        }
    }
}
