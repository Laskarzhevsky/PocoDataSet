using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void RejectChangesTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create data table with one row in Added state
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow departmentDataRow = departmentDataTable.AddNewRow();
            departmentDataRow["Id"] = 1;
            departmentDataRow["Name"] = "Reception";

            // 3. Create data table with one row in Modified state
            IDataTable employmentTypeDataTable = dataSet.AddNewTable("EmploymentType");
            employmentTypeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeDataTable.AddColumn("Code", DataTypeNames.STRING);
            employmentTypeDataTable.AddColumn("Description", DataTypeNames.STRING);

            IDataRow employmentTypeDataRow = employmentTypeDataTable.AddNewRow();
            employmentTypeDataRow["Id"] = 1;
            employmentTypeDataRow["Code"] = "ET01";
            employmentTypeDataRow["Description"] = "Full Time";
            employmentTypeDataRow.AcceptChanges();
            employmentTypeDataRow["Description"] = "Part Time";

            // 4. Create data table with one Deleted row
            IDataTable employeeDataTable = dataSet.AddNewTable("Employee");
            employeeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employeeDataTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeDataTable.AddColumn("LastName", DataTypeNames.STRING);

            IDataRow employeeDataRow = employeeDataTable.AddNewRow();
            employeeDataRow["Id"] = 1;
            employeeDataRow["FirstName"] = "John";
            employeeDataRow["LastName"] = "Doe";
            employeeDataTable.AcceptChanges();
            employeeDataRow.Delete();

            // Act
            // 5. Reject changes at data set level
            dataSet.RejectChanges();

            // Assert
            // Expected result:
            // - Department table without rows (Added row removed)
            // - EmploymentType row restored to Unchanged: 1, ET01, Full Time
            // - Employee row restored to Unchanged: 1, John, Doe

            Assert.Empty(departmentDataTable.Rows);
            
            Assert.Equal(DataRowState.Unchanged, employmentTypeDataRow.DataRowState);
            Assert.Equal(1, employmentTypeDataRow["Id"]);
            Assert.Equal("ET01", employmentTypeDataRow["Code"]);
            Assert.Equal("Full Time", employmentTypeDataRow["Description"]);

            Assert.Equal(DataRowState.Unchanged, employeeDataRow.DataRowState);
            Assert.Equal(1, employeeDataRow["Id"]);
            Assert.Equal("John", employeeDataRow["FirstName"]);
            Assert.Equal("Doe", employeeDataRow["LastName"]);
        }
    }
}
