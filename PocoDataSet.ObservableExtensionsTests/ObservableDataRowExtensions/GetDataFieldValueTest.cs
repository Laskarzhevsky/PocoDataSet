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
        public void GetDataFieldValueTest()
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

            // 3. Observe that indexer usage returns an objet
            object? nameAsObject = departmentObservableDataRow["Name"];

            // Act
            // 4. Call GetDataFieldValue to get strongly typed value
            string? name = departmentObservableDataRow.GetDataFieldValue<string>("Name");

            // Assert
            Assert.Equal("Sales", name);
        }
    }
}
