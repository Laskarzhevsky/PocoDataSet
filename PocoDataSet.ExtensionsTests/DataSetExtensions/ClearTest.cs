using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void ClearTest()
        {
            // Arrange
            // 1. Create data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create Employee data table and add columns
            IDataTable employeeTable = dataSet.AddNewTable("Employee");
            employeeTable.AddColumn("Id", DataTypeNames.INT32);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING);

            // 3. Add a new row
            IDataRow dataRow = employeeTable.AddNewRow();
            dataRow["Id"] = 1;
            dataRow["FirstName"] = "Sara";

            // Act
            // 4. Clear all rows from all tables (keeps schema)
            dataSet.Clear();

            // Assert
            // 5. Verify that Employee table has no rows but still has columns
            Assert.Empty(employeeTable.Rows);
            Assert.NotEmpty(employeeTable.Columns);
        }
    }
}
