using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void DeleteRowTest()
        {
            // Arrange
            // 1. Create an empty data set and a table
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable employeeDataTable = dataSet.AddNewTable("Employee");
            employeeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employeeDataTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeDataTable.AddColumn("LastName", DataTypeNames.STRING);

            // 2. Add a row in Added state
            IDataRow employeeDataRow1 = employeeDataTable.AddNewRow();
            employeeDataRow1["Id"] = 1;
            employeeDataRow1["FirstName"] = "John";
            employeeDataRow1["LastName"] = "Doe";

            // 3. Add a row in Unchanged state
            IDataRow employeeDataRow2 = employeeDataTable.AddNewRow();
            employeeDataRow2["Id"] = 2;
            employeeDataRow2["FirstName"] = "Sara";
            employeeDataRow2["LastName"] = "Gor";
            employeeDataRow2.AcceptChanges();

            // 3. Add a row in Modified state
            IDataRow employeeDataRow3 = employeeDataTable.AddNewRow();
            employeeDataRow3["Id"] = 1;
            employeeDataRow3["FirstName"] = "Paul";
            employeeDataRow3["LastName"] = "Carry";
            employeeDataRow3.AcceptChanges();
            employeeDataRow3["FirstName"] = "Tom";

            // Act
            // Added row removed from table
            employeeDataTable.DeleteRow(employeeDataRow1);

            // Unchanged row marked as deleted
            employeeDataTable.DeleteRow(employeeDataRow2);

            // Modified row marked as deleted
            employeeDataTable.DeleteRow(employeeDataRow3);

            // Assert
            Assert.Equal(2, employeeDataTable.Rows.Count);
            Assert.Equal(DataRowState.Deleted, employeeDataRow2.DataRowState);
            Assert.Equal(DataRowState.Deleted, employeeDataRow3.DataRowState);
        }
    }
}
