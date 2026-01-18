using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void AddColumnsFromInterfaceTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Add an empty table
            IDataTable employmentTypeDataTable = dataSet.AddNewTable("EmploymentType");

            // Act
            // 3. Add columns from the POCO interface
            employmentTypeDataTable.AddColumnsFromInterface(typeof(IEmploymentType));

            // Assert
            Assert.Equal(3, employmentTypeDataTable.Columns.Count);
        }
    }
}
