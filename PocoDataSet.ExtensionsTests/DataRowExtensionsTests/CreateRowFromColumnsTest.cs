using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void CreateRowFromColumnsTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create a table and define its columns
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // Act
            // 3. Create a row from the table columns (all values are null)
            IDataRow departmentDataRow = DataRowExtensions.CreateRowFromColumns(departmentDataTable.Columns);

            // Assert
            // 4. Observe that data row is created according with table scema but hadn't been added to the table
            Assert.True(departmentDataRow.ContainsKey("Id"));
            Assert.Null(departmentDataRow["Id"]);
            Assert.True(departmentDataRow.ContainsKey("Name"));
            Assert.Null(departmentDataRow["Name"]);
            Assert.Empty(departmentDataTable.Rows);
        }
    }
}
