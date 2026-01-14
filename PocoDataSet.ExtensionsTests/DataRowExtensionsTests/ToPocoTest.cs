using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void ToPocoTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create EmploymentType table and add a new row
            IDataTable employmentTypeDataTable = dataSet.AddNewTable("EmploymentType");
            employmentTypeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeDataTable.AddColumn("Code", DataTypeNames.STRING);
            employmentTypeDataTable.AddColumn("Description", DataTypeNames.STRING);

            IDataRow employmentTypeDataRow = employmentTypeDataTable.AddNewRow();
            employmentTypeDataRow["Id"] = 1;
            employmentTypeDataRow["Code"] = "ET01";
            employmentTypeDataRow["Description"] = "Full Time";

            // Act
            // 3. Convert row to POCO
            EmploymentType employmentTypePoco = employmentTypeDataRow.ToPoco<EmploymentType>();

            // 4. Change Code in POCO
            employmentTypePoco.Code = "ET03";

            // 5. Verify that changes are NOT propagated to data row (expected "ET01")
            string? rowCode = employmentTypeDataRow["Code"] as string;

            // Assert
            Assert.Equal("ET01", rowCode);
        }
    }
}
