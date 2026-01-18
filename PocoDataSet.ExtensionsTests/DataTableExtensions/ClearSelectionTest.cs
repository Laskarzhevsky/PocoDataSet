using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void ClearSelectionTest()
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
            employeeDataRow1.Selected = true;

            IDataRow employeeDataRow2 = employeeTable.AddNewRow();
            employeeDataRow2["Id"] = 2;
            employeeDataRow2["FirstName"] = "Sara";
            employeeDataRow2["LastName"] = "Gor";
            employeeDataRow2.Selected = true;

            // Act
            employeeTable.ClearSelection();

            // Assert
            Assert.False(employeeDataRow1.Selected);
            Assert.False(employeeDataRow2.Selected);
        }
    }
}
