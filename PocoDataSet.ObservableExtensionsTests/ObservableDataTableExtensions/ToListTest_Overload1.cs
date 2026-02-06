using System.Collections.Generic;

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
        public void ToListTest_Overload1()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table with row
            IObservableDataTable employmentTypeObservableDataTable = observableDataSet.AddNewTable("EmploymentType");
            employmentTypeObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            employmentTypeObservableDataTable.AddColumn("Code", DataTypeNames.STRING);
            employmentTypeObservableDataTable.AddColumn("Description", DataTypeNames.STRING);

            IObservableDataRow employmentTypeObservableDataRow1 = employmentTypeObservableDataTable.AddNewRow();
            employmentTypeObservableDataRow1["Id"] = 1;
            employmentTypeObservableDataRow1["Code"] = "ET01";
            employmentTypeObservableDataRow1["Description"] = "Part Time";

            // Act
            // 3. Call ToList method
            List<IObservableDataRow> observableDataRows = employmentTypeObservableDataTable.ToList();

            // Assert
            // - List has three observable rows
            Assert.Single(observableDataRows);
        }
    }
}
