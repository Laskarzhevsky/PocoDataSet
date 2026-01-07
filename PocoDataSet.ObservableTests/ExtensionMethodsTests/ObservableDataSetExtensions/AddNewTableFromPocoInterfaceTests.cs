using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests.ObservableDataSetExtensions
{
    public partial class ObservableDataSetExtensionsTests
    {
        [Fact]
        public void AddNewTableFromPocoInterface()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // Act
            // 2. Add Department observable table to observable data set
            IObservableDataTable employmentTypeObservableDataTable = observableDataSet.AddNewTableFromPocoInterface("EmploymentType", typeof(IEmploymentType));

            // Assert
            // EmploymentType observable data table is added
            Assert.NotNull(employmentTypeObservableDataTable);
        }
    }
}
