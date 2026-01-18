using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void AsInterfaceTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create an empty data table from POCO interface
            IDataTable employmentTypeDataTable = dataSet.AddNewTableFromPocoInterface("EmploymentType", typeof(IEmploymentType));

            // 3. Add a row and set values
            IDataRow employmentTypeDataRow = employmentTypeDataTable.AddNewRow();
            employmentTypeDataRow["Id"] = 1;
            employmentTypeDataRow["Code"] = "ET01";
            employmentTypeDataRow["Description"] = "Full Time";

            // Act
            // 4. Get data from data row using POCO interface (live projection)
            IEmploymentType? employmentType = employmentTypeDataTable.AsInterface<IEmploymentType>(0);

            // Assert
            Assert.NotNull(employmentType);
            Assert.Equal("ET01", employmentType.Code);

            // 5. Change data through the interface
            employmentType.Code = "ET02";

            Assert.Equal("ET02", employmentTypeDataRow["Code"]);
        }
    }
}
