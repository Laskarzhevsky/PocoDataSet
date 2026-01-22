using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataSetExtensions
{
    public partial class ObservableDataSetExtensionsTests
    {
        [Fact]
        public void RejectChangesTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table with one row in Added state
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Reception";

            // 3. Create EmploymentType observable data table with one row in Modified state
            IObservableDataTable employmentTypeObservableDataTable = observableDataSet.AddNewTable("EmploymentType");
            employmentTypeObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeObservableDataTable.AddColumn("Code", DataTypeNames.STRING);
            employmentTypeObservableDataTable.AddColumn("Description", DataTypeNames.STRING);

            IObservableDataRow employmentTypeObservableDataRow = employmentTypeObservableDataTable.AddNewRow();
            employmentTypeObservableDataRow["Id"] = 1;
            employmentTypeObservableDataRow["Code"] = "ET01";
            employmentTypeObservableDataRow["Description"] = "Full Time";
            employmentTypeObservableDataRow.AcceptChanges();
            employmentTypeObservableDataRow["Description"] = "Part Time";

            // 4. Create Employee observable data table with one row in Deleted state
            IObservableDataTable employeeObservableDataTable = observableDataSet.AddNewTable("Employee");
            employeeObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            employeeObservableDataTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeObservableDataTable.AddColumn("LastName", DataTypeNames.STRING);

            IObservableDataRow employeeObservableDataRow = employeeObservableDataTable.AddNewRow();
            employeeObservableDataRow["Id"] = 1;
            employeeObservableDataRow["FirstName"] = "John";
            employeeObservableDataRow["LastName"] = "Doe";
            employeeObservableDataTable.AcceptChanges();
            employeeObservableDataRow["FirstName"] = "Paul";
            employeeObservableDataRow.Delete();

            // Event counters
            RowsRemovedEventHandler rowsRemovedEventHandler = new RowsRemovedEventHandler();
            RowStateChangedEventHandler rowStateChangedEventHandler = new RowStateChangedEventHandler();

            departmentObservableDataTable.RowsRemoved += rowsRemovedEventHandler.Handle;
            employmentTypeObservableDataTable.RowStateChanged += rowStateChangedEventHandler.Handle;
            employeeObservableDataTable.RowStateChanged += rowStateChangedEventHandler.Handle;

            // Act
            // 5. Reject changes at data set level
            observableDataSet.RejectChanges();

            // Assert
            // Expected result:
            // - Department table without rows (Added row removed)
            // - EmploymentType row restored to Unchanged: 1, ET01, Full Time
            // - Employee row restored to Unchanged: 1, John, Doe

            Assert.Empty(departmentObservableDataTable.Rows);

            Assert.Equal(DataRowState.Unchanged, employmentTypeObservableDataRow.DataRowState);
            Assert.Equal("Full Time", employmentTypeObservableDataRow["Description"]);
            Assert.Equal(1, rowsRemovedEventHandler.GetEventCount());
            Assert.Equal(1, rowStateChangedEventHandler.GetEventCount(employmentTypeObservableDataRow, DataRowState.Modified, DataRowState.Unchanged));

            Assert.Equal("John", employeeObservableDataRow["FirstName"]);
            Assert.Equal(DataRowState.Unchanged, employeeObservableDataRow.DataRowState);
            Assert.Equal(1, rowStateChangedEventHandler.GetEventCount(employeeObservableDataRow, DataRowState.Deleted, DataRowState.Unchanged));
        }
    }
}
