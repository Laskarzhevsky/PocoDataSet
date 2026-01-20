using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataSetExtensions
{
    public partial class ObservableDataSetExtensionsTests
    {
        [Fact]
        public void AddNewTableTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // Act
            // 2. Add Department observable table to observable data set
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");

            // Assert
            // Departmenttable observable data table is added
            Assert.NotNull(departmentObservableDataTable);
        }
    }
}
