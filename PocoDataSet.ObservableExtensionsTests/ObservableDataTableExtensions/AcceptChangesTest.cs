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
        public void AcceptChangesTest()
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
            // 5. Accept changes at data set level
            observableDataSet.AcceptChanges();

            // Assert
            // - Department: two Unchanged row (1, Reception and 3, Emergency)
            Assert.Equal(2, departmentObservableDataTable.Rows.Count);
            Assert.Equal(1, rowsRemovedEventHandler.GetEventCount());

            Assert.Equal(DataRowState.Unchanged, departmentObservableDataRow1.DataRowState);
            Assert.Equal(1, departmentObservableDataRow1["Id"]);
            Assert.Equal("Reception", departmentObservableDataRow1["Name"]);
            Assert.Equal(1, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow1, DataRowState.Added, DataRowState.Unchanged));

            Assert.Equal(DataRowState.Unchanged, departmentObservableDataRow3.DataRowState);
            Assert.Equal(3, departmentObservableDataRow3["Id"]);
            Assert.Equal("Finance", departmentObservableDataRow3["Name"]);
            Assert.Equal(1, rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow3, DataRowState.Modified, DataRowState.Unchanged));
        }
    }
}
