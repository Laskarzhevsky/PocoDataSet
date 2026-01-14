using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void CreateChangesetTest()
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

            // 4. Create data table with one row that will be deleted
            IDataTable employeeDataTable = dataSet.AddNewTable("Employee");
            employeeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employeeDataTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeDataTable.AddColumn("LastName", DataTypeNames.STRING);

            IDataRow employeeDataRow = employeeDataTable.AddNewRow();
            employeeDataRow["Id"] = 1;
            employeeDataRow["FirstName"] = "Sara";
            employeeDataRow["LastName"] = "Gor";
            employeeDataTable.AcceptChanges();
            employeeDataRow.Delete();

            // Act
            // 5. Build a changeset that contains only changed rows
            IDataSet? changeset = dataSet.CreateChangeset();

            IDataTable departmentDataTableFromChangeset = changeset!.Tables["Department"];
            IDataRow departmentDataRowFromChangeset = departmentDataTableFromChangeset.Rows[0];

            IDataTable employmentTypeDataTableChangeset = changeset!.Tables["EmploymentType"];
            IDataRow employmentTypeDataRowFromChangeset = employmentTypeDataTableChangeset.Rows[0];

            IDataTable employeeDataTableChangeset = changeset!.Tables["Employee"];
            IDataRow employeeDataRowFromChangeset = employeeDataTableChangeset.Rows[0];


            // Assert
            Assert.True(departmentDataRowFromChangeset.ContainsKey("Id"));
            Assert.True(departmentDataRowFromChangeset.ContainsKey("Name"));

            Assert.True(employmentTypeDataRowFromChangeset.ContainsKey("Id"));
            Assert.False(employmentTypeDataRowFromChangeset.ContainsKey("Code"));
            Assert.True(employmentTypeDataRowFromChangeset.ContainsKey("Description"));

            Assert.True(employeeDataRowFromChangeset.ContainsKey("Id"));
            Assert.False(employeeDataRowFromChangeset.ContainsKey("FirstName"));
            Assert.False(employeeDataRowFromChangeset.ContainsKey("LastName"));
        }
    }
}
