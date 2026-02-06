using System;
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
        public void ToDetachedListTest()
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
            // 3. Call ToDetachedList method
            List<IEmploymentType> employmentTypes = employmentTypeObservableDataTable.ToDetachedList<IEmploymentType>();

            // 4. Verify that employment type is detached (read only)
            Assert.Throws<NotSupportedException>(() => employmentTypes[0].Code = "ET99");
        }
    }
}
