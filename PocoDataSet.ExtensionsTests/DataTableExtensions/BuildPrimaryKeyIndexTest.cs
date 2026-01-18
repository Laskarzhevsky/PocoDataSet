using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void BuildPrimaryKeyIndexTest()
        {
            // Arrange
            // 1. Create an empty data set and a table
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable employeeTable = dataSet.AddNewTable("Employee");
            employeeTable.AddColumn("Id", DataTypeNames.INT32);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeTable.AddColumn("LastName", DataTypeNames.STRING);

            // 2. Add several rows
            IDataRow employeeDataRow1 = employeeTable.AddNewRow();
            employeeDataRow1["Id"] = 1;
            employeeDataRow1["FirstName"] = "John";
            employeeDataRow1["LastName"] = "Doe";

            IDataRow employeeDataRow2 = employeeTable.AddNewRow();
            employeeDataRow2["Id"] = 2;
            employeeDataRow2["FirstName"] = "Sara";
            employeeDataRow2["LastName"] = "Gor";

            // Act
            // 4. Build primary key index for refreshed rows
            Dictionary<string, IDataRow> employeeTablePrimaryKeyIndex = employeeTable.BuildPrimaryKeyIndex(employeeTable.PrimaryKeys);

            // 5. Get data rows by its indexes
            IDataRow firstDataRow = employeeTablePrimaryKeyIndex["1#1"];
            IDataRow secondDataRow = employeeTablePrimaryKeyIndex["1#2"];

            // Assert
            Assert.Equal(2, employeeTablePrimaryKeyIndex.Count);
            Assert.Equal(employeeDataRow1, firstDataRow);
            Assert.Equal(employeeDataRow2, secondDataRow);
        }
    }
}
