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
        public void GetDataFieldValueTest()
        {
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create an empty Department table
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // 3. Add a new row to the Department table
            IDataRow departmentDataRow = departmentDataTable.AddNewRow();
            departmentDataRow["Id"] = 1;
            departmentDataRow["Name"] = "Customer Service";
            departmentDataTable.AddRow(departmentDataRow);

            // Act
            // 4. Read back a value from the row, observe "Customer Service" as a name<br />
            string? name = departmentDataRow.GetDataFieldValue<string>("Name");

            // Assert
            Assert.Equal("Customer Service", name);
        }
    }
}
