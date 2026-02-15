using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void DoRefreshMergePreservingLocalChanges_MergesOtherTables_AndAddsMissingTables()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();

            IDataTable currentDepartment = current.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = currentDepartment.AddNewRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Engineering";
            currentRow.AcceptChanges();

            // Refreshed dataset contains Department update + a new table.
            IDataSet refreshed = DataSetFactory.CreateDataSet();

            IDataTable refreshedDepartment = refreshed.AddNewTable("Department");
            refreshedDepartment.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Engineering Updated";
            refreshedRow.AcceptChanges();

            IDataTable refreshedEmployee = refreshed.AddNewTable("Employee");
            refreshedEmployee.AddColumn("Id", DataTypeNames.INT32);
            refreshedEmployee.AddColumn("Name", DataTypeNames.STRING);

            IDataRow emp = refreshedEmployee.AddNewRow();
            emp["Id"] = 100;
            emp["Name"] = "Sara";
            emp.AcceptChanges();

            IMergeOptions options = new MergeOptions();

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Equal("Engineering Updated", currentDepartment.Rows[0].GetDataFieldValue<string>("Name"));

            IDataTable? employee;
            bool ok = current.TryGetTable("Employee", out employee);
            Assert.True(ok);
            Assert.NotNull(employee);
            Assert.Single(employee!.Rows);
            Assert.Equal(100, employee.Rows[0].GetDataFieldValue<int>("Id"));
        }
    }
}
