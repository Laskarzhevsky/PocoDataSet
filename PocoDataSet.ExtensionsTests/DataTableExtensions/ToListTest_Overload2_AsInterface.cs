using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void ToListTest_Overload2_AsInterface()
        {
            // Arrange
            // 1. Create data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Add employment type table to data set
            IDataTable employmentTypeDataTable = dataSet.AddNewTable("EmploymentType");
            employmentTypeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeDataTable.AddColumn("Code", DataTypeNames.STRING);
            employmentTypeDataTable.AddColumn("Description", DataTypeNames.STRING);

            // 3. Add several rows to employment type table
            IDataRow employmentTypeDataRow1 = employmentTypeDataTable.AddNewRow();
            employmentTypeDataRow1["Id"] = 1;
            employmentTypeDataRow1["Code"] = "ET01";
            employmentTypeDataRow1["Description"] = "Full Time";

            IDataRow employmentTypeDataRow2 = employmentTypeDataTable.AddNewRow();
            employmentTypeDataRow2["Id"] = 2;
            employmentTypeDataRow2["Code"] = "ET02";
            employmentTypeDataRow2["Description"] = "Part Time";

            // Act
            // 4. Call ToList method providing the selector to get live proxies backed by IDataRow
            List<IEmploymentType> employmentTypes = employmentTypeDataTable.ToList(row => row.AsInterface<IEmploymentType>());

            // Assert
            Assert.Equal(2, employmentTypes.Count);
            Assert.Equal("ET02", employmentTypeDataRow2["Code"]);

            // 5. Change data of the second row through the interface
            employmentTypes[1].Description = "Contractor";

            Assert.Equal("Contractor", employmentTypeDataRow2["Description"]);
        }
    }
}
