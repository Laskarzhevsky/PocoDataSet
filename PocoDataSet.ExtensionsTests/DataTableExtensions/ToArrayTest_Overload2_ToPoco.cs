using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void ToArrayTest_Overload2_ToPoco()
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
            // 4. Call ToArray method providing the selector to get stand alone POCOs
            EmploymentType[] employmentTypes = employmentTypeDataTable.ToArray(row => row.ToPoco<EmploymentType>());

            // Assert
            // Sanity (prove snapshot contains expected values at capture time)
            Assert.Equal("ET01", employmentTypes[0].Code);
            Assert.Equal("ET02", employmentTypes[1].Code);
            Assert.Equal("Full Time", employmentTypes[0].Description);
            Assert.Equal("Part Time", employmentTypes[1].Description);

            // -----------------------------
            // Case 1: Row field value does not change if POCO property changes
            // -----------------------------
            employmentTypes[0].Code = "ET99";
            employmentTypes[0].Description = "Changed in POCO";

            // Row should remain unchanged
            Assert.Equal("ET01", (string)employmentTypeDataTable.Rows[0]["Code"]!);
            Assert.Equal("Full Time", (string)employmentTypeDataTable.Rows[0]["Description"]!);

            // -----------------------------
            // Case 2: POCO property does not change if row field value changes
            // -----------------------------
            // Capture expected snapshot values BEFORE mutating the row (avoid mixing live and snapshot reads)
            string expectedCodeRow2 = employmentTypes[1].Code;
            string? expectedDescriptionRow2 = employmentTypes[1].Description;

            // Mutate the underlying row
            employmentTypeDataTable.Rows[1]["Code"] = "ET77";
            employmentTypeDataTable.Rows[1]["Description"] = "Contractor";

            // POCO should remain unchanged (snapshot semantics)
            Assert.Equal(expectedCodeRow2, employmentTypes[1].Code);
            Assert.Equal(expectedDescriptionRow2, employmentTypes[1].Description);

            // And the row should have the new values (prove the change actually happened)
            Assert.Equal("ET77", (string)employmentTypeDataTable.Rows[1]["Code"]!);
            Assert.Equal("Contractor", (string)employmentTypeDataTable.Rows[1]["Description"]!);
        }
    }
}
