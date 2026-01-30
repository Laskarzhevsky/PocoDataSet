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
        public void ClearRowsTest()
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
            departmentObservableDataRow2.Delete();

            // Row in Modified state
            IObservableDataRow departmentObservableDataRow3 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow3["Id"] = 3;
            departmentObservableDataRow3["Name"] = "Emergency";
            departmentObservableDataRow3.AcceptChanges();
            departmentObservableDataRow3["Name"] = "Finance";

            // Event counters
            RowsRemovedEventHandler rowsRemovedEventHandler = new RowsRemovedEventHandler();
            departmentObservableDataTable.RowsRemoved += rowsRemovedEventHandler.Handle;

            // Act
            // 3. Discard current rows (for example, user starts a new search)
            departmentObservableDataTable.ClearRows();

            // Assert
            // - Department table without rows
            Assert.Empty(departmentObservableDataTable.Rows);
            Assert.Equal(3, rowsRemovedEventHandler.GetEventCount());
        }
    }
}
