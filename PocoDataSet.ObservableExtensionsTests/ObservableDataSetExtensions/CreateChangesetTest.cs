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
        public void CreateChangesetTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable data table with one row in Unchanged state and one wor in Added state
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Reception";
            departmentObservableDataRow.AcceptChanges();

            departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 2;
            departmentObservableDataRow["Name"] = "Emergency";

            // 3. Create EmploymentType observable data table with one row in Unchanged state and one row in Modified state
            IObservableDataTable employmentTypeObservableDataTable = observableDataSet.AddNewTable("EmploymentType");
            employmentTypeObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeObservableDataTable.AddColumn("Code", DataTypeNames.STRING);
            employmentTypeObservableDataTable.AddColumn("Description", DataTypeNames.STRING);

            IObservableDataRow employmentTypeObservableDataRow = employmentTypeObservableDataTable.AddNewRow();
            employmentTypeObservableDataRow["Id"] = 1;
            employmentTypeObservableDataRow["Code"] = "ET01";
            employmentTypeObservableDataRow["Description"] = "Full Time";
            employmentTypeObservableDataRow.AcceptChanges();

            employmentTypeObservableDataRow["Id"] = 2;
            employmentTypeObservableDataRow["Code"] = "ET02";
            employmentTypeObservableDataRow["Description"] = "Contract";
            employmentTypeObservableDataRow.AcceptChanges();
            employmentTypeObservableDataRow["Description"] = "Part Time";

            // 4. Create Employee observa ble data table with one row in Unchanged state and one row in Deleted state
            IObservableDataTable employeeObservableDataTable = observableDataSet.AddNewTable("Employee");
            employeeObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            employeeObservableDataTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeObservableDataTable.AddColumn("LastName", DataTypeNames.STRING);

            IObservableDataRow employeeObservableDataRow = employeeObservableDataTable.AddNewRow();
            employeeObservableDataRow["Id"] = 1;
            employeeObservableDataRow["FirstName"] = "Sara";
            employeeObservableDataRow["LastName"] = "Gor";
            employeeObservableDataTable.AcceptChanges();

            employeeObservableDataRow = employeeObservableDataTable.AddNewRow();
            employeeObservableDataRow["Id"] = 2;
            employeeObservableDataRow["FirstName"] = "Paul";
            employeeObservableDataRow["LastName"] = "Murray";
            employeeObservableDataTable.AcceptChanges();
            employeeObservableDataRow.Delete();

            // Act
            // 5. Build a changeset that contains only changed rows
            IDataSet? changeset = observableDataSet.InnerDataSet.CreateChangeset();

            // Assert
            Assert.NotNull(changeset);

            // All tables have one row only and the value of Id field of every row is 2
            Assert.Single(changeset.Tables["Department"].Rows);
            Assert.Equal(2, changeset.GetFieldValue<int>("Department", 0, "Id"));

            Assert.Single(changeset.Tables["EmploymentType"].Rows);
            Assert.Equal(2, changeset.GetFieldValue<int>("EmploymentType", 0, "Id"));

            Assert.Single(changeset.Tables["Employee"].Rows);
            Assert.Equal(2, changeset.GetFieldValue<int>("Employee", 0, "Id"));
        }
    }
}
