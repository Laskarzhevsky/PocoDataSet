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
        public void AsInterfaceTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create a table from the POCO interface
            IDataTable employmentTypeDataTable = dataSet.AddNewTableFromPocoInterface("EmploymentType", typeof(IEmploymentType));

            // 3. Add a row
            IDataRow employmentTypeDataRow = employmentTypeDataTable.AddNewRow();
            employmentTypeDataRow["Id"] = 1;
            employmentTypeDataRow["Code"] = "ET01";
            employmentTypeDataRow["Description"] = "Full Time";

            // Act
            // 4. Access the row as a strongly-typed interface
            IEmploymentType? employmentType = dataSet.AsInterface<IEmploymentType>("EmploymentType", 0);

            // Assert
            Assert.NotNull(employmentType);
            Assert.Equal(1, employmentType.Id);
            Assert.Equal("ET01", employmentType.Code);
            Assert.Equal("Full Time", employmentType.Description);

            // 5. Modify data through the interface
            employmentType.Code = "ET02";

            // 6. Changes are reflected in the underlying row
            Assert.Equal("ET02", employmentTypeDataRow["Code"]);
        }
    }
}
