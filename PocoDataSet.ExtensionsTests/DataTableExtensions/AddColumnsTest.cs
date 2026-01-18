using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void AddColumnsTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create list of column metadata
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
            columnMetadata.ColumnName = "LastName";
            columnMetadata.DataType = DataTypeNames.STRING;
            listOfColumnMetadata.Add(columnMetadata);

            // 3. Create a new table
            IDataTable employeeDataTable = dataSet.AddNewTable("Employee");

            // Act
            // 4. Add columns to the Employee table
            employeeDataTable.AddColumns(listOfColumnMetadata);

            // Assert
            Assert.Equal(3, employeeDataTable.Columns.Count);
        }
    }
}
