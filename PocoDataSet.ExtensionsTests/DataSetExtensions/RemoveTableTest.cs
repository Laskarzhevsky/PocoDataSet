using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void RemoveTableTest()
        {
            // 1. Create data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Add Department table to data set
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");

            // Act
            // 3. Remove the table by name
            dataSet.RemoveTable("Department");

            // Assert
            // Data set does not contains Department table.
            Assert.False(dataSet.Tables.ContainsKey("Department"));
        }
    }
}
