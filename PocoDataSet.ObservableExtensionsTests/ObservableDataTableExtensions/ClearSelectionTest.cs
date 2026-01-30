using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataTableExtensions
{
    public partial class ObservableDataTableExtensionsTests
    {
        [Fact]
        public void ClearSelectionTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table with rows in different states
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            // Row in Added state
            IObservableDataRow departmentObservableDataRow1 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow1["Id"] = 1;
            departmentObservableDataRow1["Name"] = "Reception";

            // Row in Deleted state
            IObservableDataRow departmentObservableDataRow2 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow2["Id"] = 2;
            departmentObservableDataRow2["Name"] = "Emergency";
            departmentObservableDataRow2.AcceptChanges();
            departmentObservableDataRow2.Selected = true;
            departmentObservableDataRow2.Delete();

            // Row in Modified state
            IObservableDataRow departmentObservableDataRow3 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow3["Id"] = 3;
            departmentObservableDataRow3["Name"] = "Emergency";
            departmentObservableDataRow3.AcceptChanges();
            departmentObservableDataRow3.Selected = true;
            departmentObservableDataRow3["Name"] = "Finance";

            // Act
            // 3. Clear selection for all rows in the table
            departmentObservableDataTable.ClearSelection();

            // Assert
            // - Department table without rows
            Assert.Equal(3, departmentObservableDataTable.Rows.Count);
            Assert.False(departmentObservableDataRow2.Selected);
            Assert.False(departmentObservableDataRow3.Selected);
        }
    }
}
