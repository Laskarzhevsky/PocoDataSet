using System;
using System.Collections.Generic;

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
        public void RejectChangesTest_RowInDeletedState()
        {
            // Arrange
            // 1. Create a new observable data
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create observable table and row
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Sales";
            departmentObservableDataRow.AcceptChanges();

            // 3. Call Delete method to put row into Deleted state
            departmentObservableDataRow.Delete();

            // Act
            // 5. Call RejectChanges to put row back into Unchanged state
            departmentObservableDataRow.RejectChanges();
            Assert.Equal(DataRowState.Unchanged.ToString(), departmentObservableDataRow.DataRowState.ToString());
        }
    }
}
