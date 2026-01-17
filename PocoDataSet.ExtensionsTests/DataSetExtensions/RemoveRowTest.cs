using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void RemoveRowTest()
        {
            // 1. Create data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Add Department table to data set
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // 3. Add two rows to Department table
            IDataRow departmentFirstDataRow = departmentDataTable.AddNewRow();
            departmentFirstDataRow["Id"] = 1;
            departmentFirstDataRow["Name"] = "Sales";

            IDataRow departmentSecondDataRow = departmentDataTable.AddNewRow();
            departmentSecondDataRow["Id"] = 2;
            departmentSecondDataRow["Name"] = "Engineering";

            // Act
            // 4a. Remove by index
            dataSet.RemoveRow("Department", 0);

            // 4.b. Remove by row reference (state-aware semantics)
            dataSet.RemoveRow("Department", departmentSecondDataRow);

            // Assert
            // Department table contain no rows.
            Assert.Empty(departmentDataTable.Rows);
        }
    }
}
