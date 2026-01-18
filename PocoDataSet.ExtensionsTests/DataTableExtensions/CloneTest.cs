using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void CloneTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create an empty Department table
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // 3. Add several rows to the Department table
            IDataRow departmentDataRow = departmentDataTable.AddNewRow();
            departmentDataRow["Id"] = 1;
            departmentDataRow["Name"] = "Customer Service";

            departmentDataRow = departmentDataTable.AddNewRow();
            departmentDataRow["Id"] = 2;
            departmentDataRow["Name"] = "Financial";

            // Act
            // 4. Clone the table (schema + rows)
            IDataTable? clonedDepartmentDataTable = departmentDataTable.Clone();

            IDataRow clonedDepartmentDataRow1 = clonedDepartmentDataTable!.Rows[0];
            IDataRow clonedDepartmentDataRow2 = clonedDepartmentDataTable.Rows[1];

            // Assert
            // SpecialColumnNames.CLIENT_KEY column is added by departmentDataTable.AddNewRow() call
            Assert.Equal(3, clonedDepartmentDataTable!.Columns.Count);
            Assert.Equal(2, clonedDepartmentDataTable.Rows.Count);

            Assert.Equal(1, clonedDepartmentDataRow1["Id"]);
            Assert.Equal("Customer Service", clonedDepartmentDataRow1["Name"]);

            Assert.Equal(2, clonedDepartmentDataRow2["Id"]);
            Assert.Equal("Financial", clonedDepartmentDataRow2["Name"]);
        }
    }
}
