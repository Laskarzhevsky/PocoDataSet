using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void AsInterfaceMethod_WithNameMapTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create EmploymentType table
            IDataTable employmentTypeDataTable = dataSet.AddNewTable("EmploymentType");
            employmentTypeDataTable.AddColumn("EmploymentTypeId", DataTypeNames.INT32);
            employmentTypeDataTable.AddColumn("EmploymentTypeCode", DataTypeNames.STRING);
            employmentTypeDataTable.AddColumn("EmploymentTypeDescription", DataTypeNames.STRING);

            // 3. Create a new row and set values
            IDataRow employmentTypeDataRow = employmentTypeDataTable.AddNewRow();
            employmentTypeDataRow["EmploymentTypeId"] = 1;
            employmentTypeDataRow["EmploymentTypeCode"] = "ET01";
            employmentTypeDataRow["EmploymentTypeDescription"] = "Full Time";

            // 4. Create name map to map interface properties to data row field names
            IDictionary<string, string> nameMap = new Dictionary<string, string>();
            nameMap.Add("Id", "EmploymentTypeId");
            nameMap.Add("Code", "EmploymentTypeCode");
            nameMap.Add("Description", "EmploymentTypeDescription");

            // Act
            // 5. Get a live proxy that implements IEmploymentType
            IEmploymentType employmentType = employmentTypeDataRow.AsInterface<IEmploymentType>(nameMap);

            // 5. Change a property via the interface
            employmentType.Code = "ET02";
            string? updatedCode = employmentTypeDataRow["EmploymentTypeCode"] as string;

            // Assert
            // 6. Verify the underlying row has changed
            Assert.Equal("ET02", updatedCode);
        }
    }
}
