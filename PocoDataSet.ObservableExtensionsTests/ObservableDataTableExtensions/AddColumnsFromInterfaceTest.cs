using PocoDataSet.Extensions;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;
namespace PocoDataSet.ObservableExtensionsTests.ObservableDataTableExtensions
{
    public partial class ObservableDataTableExtensionsTests
    {
        [Fact]
        public void AddColumnsFromInterfaceTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Add EmploymentType observable data table
            IObservableDataTable employmentTypeObservableDataTable = observableDataSet.AddNewTable("EmploymentType");

            // Act
            // 3. Add columns from the POCO interface
            employmentTypeObservableDataTable.AddColumnsFromInterface<IEmploymentType>();

            // Assert
            Assert.Equal(3, employmentTypeObservableDataTable.Columns.Count);
        }
    }
}
