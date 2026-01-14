using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void AddNewTableFromPocoInterfaceTest()
        {
            // Arrange
            // 1. Create a new data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // Act
            // 2. Call AddNewTableFromPocoInterface to add a new table named "EmploymentType" with schema from IEmploymentType interface<br />
            IDataTable employmentTypeDataTable = dataSet.AddNewTableFromPocoInterface("EmploymentType", typeof(IEmploymentType));

            // Assert
            Assert.Single(dataSet.Tables);
            Assert.Equal("EmploymentType", employmentTypeDataTable.TableName);
            Assert.Equal(3, employmentTypeDataTable.Columns.Count);
            Assert.Empty(employmentTypeDataTable.Rows);
        }
    }
}
