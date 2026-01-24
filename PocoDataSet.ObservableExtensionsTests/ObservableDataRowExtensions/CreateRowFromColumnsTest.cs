using System;
using System.Collections.Generic;
using System.Linq;

using PocoDataSet.Extensions;
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
        public void CreateRowFromColumnsTests()
        {
            // Arrange
            // 1. Create an empty data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create a table and define its columns
            // 2. Create an empty table (observable)
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            // Act
            // 3. Create a row from the table columns (all values are null)
            IObservableDataRow departmentObservableDataRow = PocoDataSet.ObservableExtensions.ObservableDataRowExtensions.CreateRowFromColumns(departmentObservableDataTable.Columns);

            // Assert
            Dictionary<string, object?> dictionary = departmentObservableDataRow.EnumerateValues().ToDictionary(p => p.Key, p => p.Value);
            foreach (IColumnMetadata columnMetadata in departmentObservableDataTable.Columns)
            {
                Assert.True(dictionary.ContainsKey(columnMetadata.ColumnName));
            }
        }
    }
}
