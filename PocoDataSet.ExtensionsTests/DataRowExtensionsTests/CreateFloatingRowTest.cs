using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void CreateFloatingRowTest()
        {
            // Arrange
            // 1. Create floating data row and observe that it has no fields
            IFloatingDataRow floatingDataRow = DataRowExtensions.CreateFloatingRow();

            // Add any field of any type according to the business needs by assigning values to floating data row
            // Add FirstName field type of string
            floatingDataRow["FirstName"] = "Sara";

            // Add DepartmentId field type of int
            floatingDataRow["DepartmentId"] = 2;

            // Add DateOfBirth field with null value
            floatingDataRow["DateOfBirth"] = null;

            // and so on...
            // Floating data row can be added to any table because its schema doesn't bound to the schema of any table

            // 2. Create data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 3. Create Employee data table
            IDataTable employmentTypeDataTable = dataSet.AddNewTable("Employee");
            employmentTypeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeDataTable.AddColumn("FirstName", DataTypeNames.STRING);
            employmentTypeDataTable.AddColumn("LastName", DataTypeNames.STRING);

            // Act
            // 4. Add floating row to Employee data table
            employmentTypeDataTable.AddRow(floatingDataRow);

            // Assert
            // a) Floating row still contains fields that are NOT part of the table schema
            Assert.True(floatingDataRow.ContainsKey("DepartmentId"));
            Assert.Equal(2, floatingDataRow["DepartmentId"]);

            Assert.True(floatingDataRow.ContainsKey("DateOfBirth"));
            Assert.Null(floatingDataRow["DateOfBirth"]);

            // b) Floating row contains fields that ARE part of the table schema
            Assert.True(floatingDataRow.ContainsKey("FirstName"));
            Assert.Equal("Sara", floatingDataRow["FirstName"]);

            // c) Floating row was NOT auto-expanded to include table schema columns
            Assert.False(floatingDataRow.ContainsKey("LastName"));

            // d) Row is actually present in the table
            Assert.Single(employmentTypeDataTable.Rows);
            Assert.Same(floatingDataRow, employmentTypeDataTable.Rows[0]);
        }
    }
}
