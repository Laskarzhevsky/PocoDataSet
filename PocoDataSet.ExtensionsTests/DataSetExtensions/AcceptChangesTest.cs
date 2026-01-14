using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void AcceptChangesTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Table with one row in Added state
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow departmentDataRow = departmentDataTable.AddNewRow();
            departmentDataRow["Id"] = 1;
            departmentDataRow["Name"] = "Reception";

            // 3. Table with one row in Modified state
            IDataTable employmentTypeDataTable = dataSet.AddNewTable("EmploymentType");
            employmentTypeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeDataTable.AddColumn("Code", DataTypeNames.STRING);
            employmentTypeDataTable.AddColumn("Description", DataTypeNames.STRING);

            IDataRow employmentTypeDataRow = employmentTypeDataTable.AddNewRow();
            employmentTypeDataRow["Id"] = 1;
            employmentTypeDataRow["Code"] = "ET01";
            employmentTypeDataRow["Description"] = "Full Time";

            // Make the row Unchanged, then modify it
            employmentTypeDataRow.AcceptChanges();
            employmentTypeDataRow["Description"] = "Part Time";

            // 4. Table with one row that will be deleted
            IDataTable employeeDataTable = dataSet.AddNewTable("Employee");
            employeeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employeeDataTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeDataTable.AddColumn("LastName", DataTypeNames.STRING);

            IDataRow employeeDataRow = employeeDataTable.AddNewRow();
            employeeDataRow["Id"] = 1;
            employeeDataRow["FirstName"] = "John";
            employeeDataRow["LastName"] = "Doe";
            employeeDataRow.AcceptChanges();
            employeeDataRow.Delete();

            // Act
            // 5. Accept changes at data set level
            dataSet.AcceptChanges();

            // Assert
            // Expected result:
            // - Department: one Unchanged row (1, Reception)
            Assert.Single(departmentDataTable.Rows);
            Assert.Equal(DataRowState.Unchanged, departmentDataRow.DataRowState);

            // - EmploymentType: one Unchanged row (1, ET01, Part Time)
            Assert.Single(employmentTypeDataTable.Rows);
            Assert.Equal(DataRowState.Unchanged, employmentTypeDataRow.DataRowState);
            Assert.Equal("Part Time", employmentTypeDataRow["Description"]);

            // - Employee: deleted row removed from the table
            Assert.Empty(employeeDataTable.Rows);
        }
    }
}
