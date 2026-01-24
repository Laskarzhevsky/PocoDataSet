using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataRowExtensions
{
    public partial class ObservableDataRowExtensionsTests
    {
        [Fact]
        public void CopyFromPocoTest()
        {
            // Arrange
            IObservableDataSet observableDataSet = new ObservableDataSet();

            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING, true, false);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Sales";
            departmentObservableDataRow.AcceptChanges();

            DataFieldValueChangedEventHandler dataFieldValueChangedEventHandler = new DataFieldValueChangedEventHandler();
            RowStateChangedEventHandler rowStateChangedEventHandler = new RowStateChangedEventHandler();

            departmentObservableDataRow.DataFieldValueChanged += dataFieldValueChangedEventHandler.Handle;
            departmentObservableDataRow.RowStateChanged += rowStateChangedEventHandler.Handle;

            IDepartment department = new Department();
            department.id = 1;
            department.NAME = "Marketing";

            // Act
            departmentObservableDataRow.CopyFromPoco(department);

            //Assert
            Assert.Equal(DataRowState.Modified, departmentObservableDataRow.DataRowState);
            Assert.Equal(1, departmentObservableDataRow["Id"]);
            Assert.Equal("Marketing", departmentObservableDataRow["Name"]);

            Assert.True(dataFieldValueChangedEventHandler.GetEventCount("Name") == 1);
            Assert.True(rowStateChangedEventHandler.GetEventCount(departmentObservableDataRow, DataRowState.Unchanged, DataRowState.Modified) == 1);
        }
    }
}
