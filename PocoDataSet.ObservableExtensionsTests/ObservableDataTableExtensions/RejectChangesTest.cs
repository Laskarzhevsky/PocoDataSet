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
        public void RejectChangesTest()
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
            departmentObservableDataRow2["Name"] = "Sales";
            departmentObservableDataRow2.AcceptChanges();
            departmentObservableDataRow2.Delete();

            // Row in Modified state
            IObservableDataRow departmentObservableDataRow3 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow3["Id"] = 3;
            departmentObservableDataRow3["Name"] = "Emergency";
            departmentObservableDataRow3.AcceptChanges();
            departmentObservableDataRow3["Name"] = "Finance";

            // Sanity checks before Act (optional but helpful)
            Assert.Equal(DataRowState.Added, departmentObservableDataRow1.DataRowState);
            Assert.Equal(DataRowState.Deleted, departmentObservableDataRow2.DataRowState);
            Assert.Equal(DataRowState.Modified, departmentObservableDataRow3.DataRowState);

            // Event counters
            RowsRemovedEventHandler rowsRemovedEventHandler = new RowsRemovedEventHandler();
            RowStateChangedEventHandler rowStateChangedEventHandler = new RowStateChangedEventHandler();

            departmentObservableDataTable.RowsRemoved += rowsRemovedEventHandler.Handle;
            departmentObservableDataRow1.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow2.RowStateChanged += rowStateChangedEventHandler.Handle;
            departmentObservableDataRow3.RowStateChanged += rowStateChangedEventHandler.Handle;

            // Act
            // 5. Reject changes
            observableDataSet.RejectChanges();

            // Assert
            // - Department: two Unchanged row (2, Sales and 3, Emergency)
            Assert.Equal(2, departmentObservableDataTable.Rows.Count);
            Assert.Equal(1, rowsRemovedEventHandler.GetEventCount());

            Assert.Equal(DataRowState.Unchanged, departmentObservableDataRow2.DataRowState);
            Assert.Equal(2, departmentObservableDataRow2["Id"]);
            Assert.Equal("Sales", departmentObservableDataRow2["Name"]);
            Assert.Equal(1, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow2, DataRowState.Deleted, DataRowState.Unchanged));

            Assert.Equal(DataRowState.Unchanged, departmentObservableDataRow3.DataRowState);
            Assert.Equal(3, departmentObservableDataRow3["Id"]);
            Assert.Equal("Emergency", departmentObservableDataRow3["Name"]);
            Assert.Equal(1, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow3, DataRowState.Modified, DataRowState.Unchanged));
        }
    }
}
