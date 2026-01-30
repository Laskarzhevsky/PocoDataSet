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
        public void GetFieldValueTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table with row
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow1 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow1["Id"] = 1;
            departmentObservableDataRow1["Name"] = "Reception";

            // Act
            // 3. Get department name as string
            string? departmentName = departmentObservableDataTable.GetFieldValue<string>(0, "name");

            // Assert
            Assert.Equal("Reception", departmentName);
        }
    }
}
