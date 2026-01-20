using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataRowExtensions
{
    public partial class ObservableDataRowExtensionsTests
    {
        [Fact]
        public void AcceptChangesTest_RowInModifiedState()
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
