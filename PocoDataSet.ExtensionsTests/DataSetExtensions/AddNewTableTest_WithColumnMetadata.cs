using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void AddNewTableTest_WithColumnMetadata()
        {
            // Arrange
            // 1. Create a new data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Define list of column metadata for the "Employee" table
            List<IColumnMetadata> listOfColumnMetadata = new List<IColumnMetadata>();

            IColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = "Id";
            columnMetadata.DataType = DataTypeNames.INT32;
            columnMetadata.IsNullable = false;
            columnMetadata.IsPrimaryKey = true;
            listOfColumnMetadata.Add(columnMetadata);

            columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = "FirstName";
            columnMetadata.DataType = DataTypeNames.STRING;
            listOfColumnMetadata.Add(columnMetadata);

            columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = "DepartmentId";
            columnMetadata.DataType = DataTypeNames.INT32;
            columnMetadata.IsNullable = false;
            columnMetadata.IsForeignKey = true;
            listOfColumnMetadata.Add(columnMetadata);

            // Act
            // 3. Add a new table named "Employee" with defined columns
            IDataTable employeeDataTable = dataSet.AddNewTable("Employee", listOfColumnMetadata);

            // Assert
            Assert.Single(dataSet.Tables);
            Assert.Equal("Employee", employeeDataTable.TableName);
            Assert.Equal(3, employeeDataTable.Columns.Count);
            Assert.Empty(employeeDataTable.Rows);
        }
    }
}
