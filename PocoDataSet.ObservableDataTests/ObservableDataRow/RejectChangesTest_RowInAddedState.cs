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
        public void RejectChangesTest_RowInAddedState()
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

            // Act
            // 3. Call RejectChanges method and observe thrown InvalidOperationException
            Assert.Throws<InvalidOperationException>(() =>
            {
                departmentObservableDataRow.RejectChanges();
            });
        }
    }
}
