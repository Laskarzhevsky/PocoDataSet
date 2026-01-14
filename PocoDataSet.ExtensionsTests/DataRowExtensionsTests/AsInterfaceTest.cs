using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void AsInterfaceTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create an empty table from the POCO interface
            IDataTable employmentTypeDataTable = dataSet.AddNewTableFromPocoInterface("EmploymentType", typeof(IEmploymentType));

            // 3. Create a new row and set values
            IDataRow employmentTypeDataRow = employmentTypeDataTable.AddNewRow();
            employmentTypeDataRow["Id"] = 1;
            employmentTypeDataRow["Code"] = "ET01";
            employmentTypeDataRow["Description"] = "Full Time";

            // Act
            // 4. Get a live proxy that implements IEmploymentType
            IEmploymentType employmentType = employmentTypeDataRow.AsInterface<IEmploymentType>();

            // 5. Change a property via the interface
            employmentType.Code = "ET02";
            string? updatedCode = employmentTypeDataRow["Code"] as string;

            // Assert
            // 6. Verify the underlying row has changed
            Assert.Equal("ET02", updatedCode);
        }
    }
}
