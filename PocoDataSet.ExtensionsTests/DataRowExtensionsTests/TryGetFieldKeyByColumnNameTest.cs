using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void TryGetFieldKeyByColumnNameTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create Employee table and add some columns
            IDataTable employeeTable = dataSet.AddNewTable("Employee");
            employeeTable.AddColumn("Id", DataTypeNames.INT32);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING);

            // 3. Add a new row with a specific field key casing (e.g., "FirstName")
            IDataRow employeeRow = employeeTable.AddNewRow();
            employeeRow["Id"] = 1;
            employeeRow["FirstName"] = "Sara";

            // Act
            // 4. Try to find filed key by a different casing (e.g., "firstname")
            bool found = employeeRow.TryGetFieldKeyByColumnName("firstname", out string? fieldKey);

            // Assert
            // 5. Verify that that field key is found
            Assert.True(found);
            Assert.Equal("FirstName", fieldKey);
        }
    }
}
