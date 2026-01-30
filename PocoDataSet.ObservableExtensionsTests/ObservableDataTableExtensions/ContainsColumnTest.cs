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
        public void ContainsColumnTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table with rows in different states
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            // Act
            bool containsIdColumn = departmentObservableDataTable.ContainsColumn("ID");
            bool containsNameColumn = departmentObservableDataTable.ContainsColumn("name");
            bool containsCodeColumn = departmentObservableDataTable.ContainsColumn("Code");

            // Assert
            Assert.True(containsIdColumn);
            Assert.True(containsNameColumn);
            Assert.False(containsCodeColumn);
        }
    }
}
