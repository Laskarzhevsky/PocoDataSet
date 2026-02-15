using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void GetPrimaryKeyColumnNamesTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create an empty Department table
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // Act
            // 3. Call GetPrimaryKeyColumnNames method, expected one entry with “Id” key
            IReadOnlyList<string> dataTablePrimaryKeyColumnNames = departmentDataTable.PrimaryKeys;

            // Assert
            Assert.Single(dataTablePrimaryKeyColumnNames);
            Assert.Equal("Id", dataTablePrimaryKeyColumnNames[0]);
        }
    }
}
