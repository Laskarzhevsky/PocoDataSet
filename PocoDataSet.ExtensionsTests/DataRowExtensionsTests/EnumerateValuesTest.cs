using System.Collections.Generic;
using System.Linq;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void EnumerateValuesTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create a table and define its columns
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow departmentDataRow = departmentDataTable.AddNewRow();
            departmentDataRow["Id"] = 1;
            departmentDataRow["Name"] = "Sales";

            // Act
            // 3. Enumerate all values in the row
            Dictionary<string, object?> dictionary = departmentDataRow.EnumerateValues().ToDictionary(p => p.Key, p => p.Value);

            // Assert
            Assert.Equal(3, dictionary.Count);
            Assert.Equal(1, dictionary["Id"]);
            Assert.Equal("Sales", dictionary["Name"]);
        }
    }
}
