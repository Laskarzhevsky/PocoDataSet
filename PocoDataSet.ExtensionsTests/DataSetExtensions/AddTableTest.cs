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
        public void AddTableTest()
        {
            // Arrange
            // 1) Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2) Create a table and assign a name
            IDataTable departmentDataTable = new DataTable();
            departmentDataTable.TableName = "Department";

            // 3) Build schema / rows as needed
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // Act
            // 4) Add the table into the data set
            dataSet.AddTable(departmentDataTable);

            // Assert
            Assert.Single(dataSet.Tables);
            Assert.Equal("Department", departmentDataTable.TableName);
            Assert.Equal(2, departmentDataTable.Columns.Count);
            Assert.Empty(departmentDataTable.Rows);
        }
    }
}
