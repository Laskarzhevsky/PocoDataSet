using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void AddNewTableTest()
        {
            // Arrange
            // 1. Create a new data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // Act
            // 2. Add a new table named "Department"
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");

            // Assert
            Assert.Single(dataSet.Tables);
            Assert.Equal("Department", departmentDataTable.TableName);
            Assert.Empty(departmentDataTable.Columns);
            Assert.Empty(departmentDataTable.Rows);
        }
    }
}
