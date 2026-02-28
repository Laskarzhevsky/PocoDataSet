using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void TryGetTableTest()
        {
            // 1. Create data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Add Department table to data set
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");

            // Act
            // 3. Try to retrieve the table safely
            if (dataSet.TryGetTable("Department", out IDataTable? foundTable))
            {
                // foundTable is not null
            }

            // Assert
            // Department table retreived.
            Assert.NotNull(foundTable);
        }
    }
}
