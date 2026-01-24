using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataSetExtensions
{
    public partial class ObservableDataSetExtensionsTests
    {
        [Fact]
        public void UpdateFieldValueTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow["Id"] = 1;
            departmentObservableDataRow["Name"] = "Reception";

            // 3. Call GetFieldValue method and observe "Reception" as a name
            string? name = observableDataSet.GetFieldValue<string>("Department", 0, "Name");
            Assert.Equal("Reception", name);

            // Act
            // 4. Call UpdateFieldValue method and verify that vauls changed
            observableDataSet.UpdateFieldValue<string>("Department", 0, "Name", "Emergency");

            // Assert
            name = observableDataSet.GetFieldValue<string>("Department", 0, "Name");
            Assert.Equal("Emergency", name);
        }
    }
}
