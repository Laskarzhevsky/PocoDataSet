using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void CopyToPocoTest_Partial()
        {
            // Arrange
            // 1. Create data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create EmploymentType data table without Description column
            IDataTable employmentTypeDataTable = dataSet.AddNewTable("EmploymentType");
            employmentTypeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeDataTable.AddColumn("Code", DataTypeNames.STRING);

            IDataRow employmentTypeDataRow = employmentTypeDataTable.AddNewRow();
            employmentTypeDataRow["Id"] = 1;
            employmentTypeDataRow["Code"] = "ET01";

            // 3. Create EmploymentType POCO
            IEmploymentType employmentType = new EmploymentType();

            // Act
            // 4. Call CopyToPoco method
            employmentTypeDataRow.CopyToPoco(employmentType);

            // Assert
            // 4. Observe that all properties of POCO are assigned except Description
            Assert.Equal(1, employmentType.Id);
            Assert.Equal("ET01", employmentType.Code);
            Assert.Null(employmentType.Description);
        }
    }
}
