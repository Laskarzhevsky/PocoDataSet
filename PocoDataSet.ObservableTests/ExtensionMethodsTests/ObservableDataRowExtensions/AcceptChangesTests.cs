using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests.ObservableDataRowExtensions
{
    public partial class ObservableDataRowExtensionsTests
    {
        [Fact]
        public void AcceptChanges_RowInAddedState()
        {
            // Arrange
            // 1. Create a new observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create observable table and row
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Sales";

            // Act
            // 3. Accept the row changes
            departmentObservableDataRow.AcceptChanges();

            // Assert
            Assert.Equal(DataRowState.Unchanged.ToString(), departmentObservableDataRow.DataRowState.ToString());
            Assert.Equal("Sales", departmentObservableDataRow["Name"]);
        }

        [Fact]
        public void AcceptChanges_RowInDeletedState()
        {
            // Arrange
            // 1. Create a new observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create observable table and row
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Sales";

            // 3. Accept the row changes
            departmentObservableDataRow.AcceptChanges();

            // 4. Call Delete method to put row into Deleted state
            departmentObservableDataRow.Delete();

            // Act
            // 5. Call AcceptChanges method and observe that exception is thrown
            Assert.Throws<InvalidOperationException>(() =>
            {
                departmentObservableDataRow.AcceptChanges();
            });
        }

        [Fact]
        public void AcceptChanges_RowInModifiedState()
        {
            // Arrange
            // 1. Create a new observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create observable table and row
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Sales";

            // 3. Accept the row changes
            departmentObservableDataRow.AcceptChanges();

            // 4. Update row data to put row into Modified state
            departmentObservableDataRow["Name"] = "Reception";

            // Act
            // 5. Call AcceptChanges method and observe that row is in Unchanged state
            departmentObservableDataRow.AcceptChanges();

            // Assert
            Assert.Equal(DataRowState.Unchanged.ToString(), departmentObservableDataRow.DataRowState.ToString());
            Assert.Equal("Reception", departmentObservableDataRow["Name"]);
        }
    }
}
