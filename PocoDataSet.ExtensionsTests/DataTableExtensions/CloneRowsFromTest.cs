using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void CloneRowsFromTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create an empty Department table
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // 3. Add rows to the Department table
            IDataRow departmentDataRow1 = departmentDataTable.AddNewRow();
            departmentDataRow1["Id"] = 1;
            departmentDataRow1["Name"] = "Customer Service";

            IDataRow departmentDataRow2 = departmentDataTable.AddNewRow();
            departmentDataRow2["Id"] = 2;
            departmentDataRow2["Name"] = "Finance";

            // 4. Create an empty table and add schema to it
            IDataTable clonedDepartmentDataTable = new DataTable();
            clonedDepartmentDataTable.TableName = departmentDataTable.TableName;
            clonedDepartmentDataTable.CloneColumnsFrom(departmentDataTable);

            // Act
            // 5. Clone rows
            clonedDepartmentDataTable.CloneRowsFrom(departmentDataTable);

            IDataRow clonedDepartmentDataRow1 = clonedDepartmentDataTable!.Rows[0];
            IDataRow clonedDepartmentDataRow2 = clonedDepartmentDataTable.Rows[1];

            // Assert
            Assert.Equal(2, clonedDepartmentDataTable.Rows.Count);

            Assert.Equal(1, clonedDepartmentDataRow1["Id"]);
            Assert.Equal("Customer Service", clonedDepartmentDataRow1["Name"]);

            Assert.Equal(2, clonedDepartmentDataRow2["Id"]);
            Assert.Equal("Finance", clonedDepartmentDataRow2["Name"]);
        }
    }
}
