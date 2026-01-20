using System.Collections.Generic;
using System.Linq;

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
        public void EnumerateValuesTest()
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
            // 3. Enumerate all values in the row
            Dictionary<string, object?> dictionary = departmentObservableDataRow.EnumerateValues().ToDictionary(p => p.Key, p => p.Value);

            // Assert
            Assert.Equal(3, dictionary.Count);
            Assert.Equal(1, dictionary["Id"]);
            Assert.Equal("Sales", dictionary["Name"]);
        }
    }
}
