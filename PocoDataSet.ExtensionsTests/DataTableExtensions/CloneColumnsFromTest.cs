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
        public void CloneColumnsFromTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create an empty Department table
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // 3. Create an empty ClonedDepartment table
            IDataTable clonedDepartmentDataTable = new DataTable();
            clonedDepartmentDataTable.TableName = departmentDataTable.TableName;

            // Act
            // 4. Clone only the column metadata (schema)
            clonedDepartmentDataTable.CloneColumnsFrom(departmentDataTable);

            // Assert
            Assert.Equal(2, clonedDepartmentDataTable.Columns.Count);
            Assert.True(clonedDepartmentDataTable.ContainsColumn("Id"));
            Assert.True(clonedDepartmentDataTable.ContainsColumn("Name"));
        }
    }
}
