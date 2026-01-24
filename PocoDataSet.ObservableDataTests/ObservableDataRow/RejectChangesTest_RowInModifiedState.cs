using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableDataTests.ObservableDataRow
{
    public partial class ObservableDataRowExtensionsTests
    {
        [Fact]
        public void RejectChangesTest_RowInModifiedState()
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

            // 3. Accept the row changes to put row into Unchenged state
            departmentObservableDataRow.AcceptChanges();

            // Department name is Sales
            string? departmentName = departmentObservableDataRow["Name"] as string;

            // 4. Modify row data
            departmentObservableDataRow["Name"] = "Reception";

            // Department name is Reception
            departmentName = departmentObservableDataRow["Name"] as string;

            // Act
            // 5. Call RejectChanges
            departmentObservableDataRow.RejectChanges();

            // Assert
            // Department name is Sales
            departmentName = departmentObservableDataRow["Name"] as string;
            Assert.Equal("Sales", departmentName);
        }
    }
}
