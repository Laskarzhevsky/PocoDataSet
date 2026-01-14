using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void CreateRowFromColumnsWithDefaultValues()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create a table and define its columns
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);
            departmentDataTable.AddColumn("DateOfCreation", DataTypeNames.DATE_TIME, false);

            // Act
            // 3. Create a row from the table columns
            IDataRow departmentDataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(departmentDataTable.Columns);

            // Assert
            // 4. Observe that data row is created according with table scema but hadn't been added to the table
            Assert.True(departmentDataRow.ContainsKey("Id"));
            Assert.Equal(0, departmentDataRow["Id"]);
            Assert.True(departmentDataRow.ContainsKey("Name"));
            Assert.Equal("", departmentDataRow["Name"]);
            Assert.True(departmentDataRow.ContainsKey("DateOfCreation"));
            Assert.Equal(DateTime.MinValue, departmentDataRow["DateOfCreation"]);
            Assert.Empty(departmentDataTable.Rows);
        }
    }
}
