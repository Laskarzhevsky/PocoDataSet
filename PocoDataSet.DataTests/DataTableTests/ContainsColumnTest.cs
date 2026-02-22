using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.DataTests.DataTableTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void ContainsColumnTest()
        {
            // Arrange
            // 1. Create data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Add Department table to data set
            IDataTable departmentTable = dataSet.AddNewTable("Department");
            departmentTable.AddColumn("Id", DataTypeNames.INT32);
            departmentTable.AddColumn("Name", DataTypeNames.STRING);

            // Act
            // 2. Check for columns
            bool hasIdColumn = departmentTable.ContainsColumn("Id");
            bool hasNameColumn = departmentTable.ContainsColumn("NAME");

            // Assert
            Assert.True(hasIdColumn);
            Assert.True(hasNameColumn);
        }
    }
}
