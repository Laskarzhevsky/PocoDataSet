using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void CopyFromTest()
        {
            // Arrange
            // 1. Create data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // Create EmploymentType data table without Description column
            IDataTable employmentTypeDataTable = dataSet.AddNewTable("EmploymentType");
            employmentTypeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeDataTable.AddColumn("Code", DataTypeNames.STRING);

            // Add current data row to it
            IDataRow employmentTypeCurrentDataRow = employmentTypeDataTable.AddNewRow();

            // 2. Create refreshed data set
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();

            // Create EmploymentType data table with Description column and refreshed data
            IDataTable employmentTypeRefreshedTable = refreshedDataSet.AddNewTable("EmploymentType");
            employmentTypeRefreshedTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeRefreshedTable.AddColumn("Code", DataTypeNames.STRING);
            employmentTypeRefreshedTable.AddColumn("Description", DataTypeNames.STRING);

            IDataRow employmentTypeRefreshedDataRow = employmentTypeRefreshedTable.AddNewRow();
            employmentTypeRefreshedDataRow["Id"] = 1;
            employmentTypeRefreshedDataRow["Code"] = "ET01";
            employmentTypeRefreshedDataRow["Description"] = "Part Time";

            // Act
            // 3. Call CopyFrom method and observe that employmentTypeCurrentDataRow contains "Description" field with "Part Time" value
            employmentTypeCurrentDataRow.CopyFrom(employmentTypeRefreshedDataRow, employmentTypeRefreshedTable.Columns);

            // Assert
            // 4. Verify the underlying row has changed
            Assert.Equal(1, employmentTypeCurrentDataRow["Id"]);
            Assert.Equal("ET01", employmentTypeCurrentDataRow["Code"]);
            Assert.Equal("Part Time", employmentTypeCurrentDataRow["Description"]);
        }
    }
}
