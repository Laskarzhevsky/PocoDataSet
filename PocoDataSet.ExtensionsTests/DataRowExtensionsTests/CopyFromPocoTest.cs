using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void CopyFromPocoTest()
        {
            // Arrange
            // 1. Create data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create EmploymentType data table from POCO interface and add a new row to it
            IDataTable employmentTypeDataTable = dataSet.AddNewTableFromPocoInterface("EmploymentType", typeof(IEmploymentType));
            IDataRow employmentTypeDataRow = employmentTypeDataTable.AddNewRow();

            // 3. Create POCO object
            EmploymentType employmentType = new EmploymentType();
            employmentType.Id = 1;
            employmentType.Code = "ET01";
            employmentType.Description = "Full Time";

            // Act
            // 4. Copy values into the row
            employmentTypeDataRow.CopyFromPoco<EmploymentType>(employmentType);

            // Assert
            // 6. Verify the underlying row has changed
            Assert.Equal("ET01", employmentTypeDataRow["Code"]);
        }
    }
}
