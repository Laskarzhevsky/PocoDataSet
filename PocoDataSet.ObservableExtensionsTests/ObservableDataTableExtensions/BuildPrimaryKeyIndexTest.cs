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
        public void BuildPrimaryKeyIndexTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table with rows in different states
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            // 3. Build primary key column names list (example helper)
            List<string> primaryKeyColumnNames = departmentObservableDataTable.GetPrimaryKeyColumnNames(null);

            // Act
            // 4. Build index for refreshed rows
            Dictionary<string, IDataRow> refreshedIndex = departmentObservableDataTable.BuildPrimaryKeyIndex(primaryKeyColumnNames);
        }
    }
}
