using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests.ObservableDataSetExtensions
{
    public partial class ObservableDataSetExtensionsTests
    {
        [Fact]
        public void AcceptChanges()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Table with one row in Added state
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Reception";

            // 3. Table with one row in Modified state
            IObservableDataTable employmentTypeObservableDataTable = observableDataSet.AddNewTable("EmploymentType");
            employmentTypeObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeObservableDataTable.AddColumn("Code", DataTypeNames.STRING);
            employmentTypeObservableDataTable.AddColumn("Description", DataTypeNames.STRING);

            IObservableDataRow employmentTypeObservableDataRow = employmentTypeObservableDataTable.AddNewRow();
            employmentTypeObservableDataRow["Id"] = 1;
            employmentTypeObservableDataRow["Code"] = "ET01";
            employmentTypeObservableDataRow["Description"] = "Full Time";

            // Make the row Unchanged, then modify it
            employmentTypeObservableDataRow.AcceptChanges();
            employmentTypeObservableDataRow["Description"] = "Part Time";

            // 4. Table with one row in Deleted state
            IObservableDataTable employeeObservableDataTable = observableDataSet.AddNewTable("Employee");
            employeeObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            employeeObservableDataTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeObservableDataTable.AddColumn("LastName", DataTypeNames.STRING);

            IObservableDataRow employeeObservableDataRow = employeeObservableDataTable.AddNewRow();
            employeeObservableDataRow["Id"] = 1;
            employeeObservableDataRow["FirstName"] = "John";
            employeeObservableDataRow["LastName"] = "Doe";
            employeeObservableDataRow.AcceptChanges();
            employeeObservableDataRow.Delete();

            // Sanity checks before Act (optional but helpful)
            Assert.Equal(DataRowState.Added, departmentObservableDataRow.DataRowState);
            Assert.Equal(DataRowState.Modified, employmentTypeObservableDataRow.DataRowState);
            Assert.Equal(DataRowState.Deleted, employeeObservableDataRow.DataRowState);

            // Act
            // 5. Accept changes at data set level
            observableDataSet.AcceptChanges();

            // Assert
            // - Department: one Unchanged row (1, Reception)
            Assert.Equal(1, departmentObservableDataTable.Rows.Count);
            Assert.Equal(DataRowState.Unchanged, departmentObservableDataTable.Rows[0].DataRowState);
            Assert.Equal(1, (int)departmentObservableDataTable.Rows[0]["Id"]);
            Assert.Equal("Reception", (string)departmentObservableDataTable.Rows[0]["Name"]);

            // - EmploymentType: one Unchanged row (1, ET01, Part Time)
            Assert.Equal(1, employmentTypeObservableDataTable.Rows.Count);
            Assert.Equal(DataRowState.Unchanged, employmentTypeObservableDataTable.Rows[0].DataRowState);
            Assert.Equal(1, (int)employmentTypeObservableDataTable.Rows[0]["Id"]);
            Assert.Equal("ET01", (string)employmentTypeObservableDataTable.Rows[0]["Code"]);
            Assert.Equal("Part Time", (string)employmentTypeObservableDataTable.Rows[0]["Description"]);

            // - Employee: deleted row removed from the table
            Assert.Equal(0, employeeObservableDataTable.Rows.Count);
        }
    }
}
