using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void AddRowTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create an empty data table
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // 3. Create a detached row from the table schema and populate it before insertion
            IDataRow departmentDataRow = DataRowExtensions.CreateRowFromColumns(departmentDataTable.Columns);
            departmentDataRow["Id"] = 1;
            departmentDataRow["Name"] = "Customer Service";

            // Act
            // 4. Insert the populated row into the table
            dataSet.AddRow("Department", departmentDataRow);

            // Assert
            Assert.Single(dataSet.Tables);
            Assert.Equal("Department", departmentDataTable.TableName);
            Assert.Equal(2, departmentDataTable.Columns.Count);
            Assert.Single(departmentDataTable.Rows);
        }
    }
}
