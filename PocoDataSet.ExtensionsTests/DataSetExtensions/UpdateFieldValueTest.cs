using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void UpdateFieldValueTest()
        {
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create a table and add a row
            IDataTable employeeTable = dataSet.AddNewTable("Employee");
            employeeTable.AddColumn("Id", DataTypeNames.INT32);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeTable.AddColumn("LastName", DataTypeNames.STRING);

            IDataRow employeeRow = employeeTable.AddNewRow();
            employeeRow["Id"] = 1;
            employeeRow["FirstName"] = "John";
            employeeRow["LastName"] = "Doe";
            employeeTable.AcceptChanges();

            // Act
            // 3. Update a field value using the data set extension
            dataSet.UpdateFieldValue("Employee", 0, "LastName", "Smith");

            // Assert
            // "Smith" value was assigned to "LastName" field ot the row with index 0
            Assert.Equal("Smith", employeeRow["LastName"]);
        }
    }
}
