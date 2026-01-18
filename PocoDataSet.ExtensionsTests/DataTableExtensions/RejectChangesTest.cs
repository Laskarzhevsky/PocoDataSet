using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void RejectChangesTest()
        {
            // Arrange
            // Arrange
            // 1. Create an empty data set and a table
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable employeeTable = dataSet.AddNewTable("Employee");
            employeeTable.AddColumn("Id", DataTypeNames.INT32);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING);
            employeeTable.AddColumn("LastName", DataTypeNames.STRING);

            // 2. Add a row (Added state)
            IDataRow employeeDataRow1 = employeeTable.AddNewRow();
            employeeDataRow1["Id"] = 1;
            employeeDataRow1["FirstName"] = "John";
            employeeDataRow1["LastName"] = "Doe";

            // 3. Add a row and then delete it (Deleted state)
            IDataRow employeeDataRow2 = employeeTable.AddNewRow();
            employeeDataRow2["Id"] = 2;
            employeeDataRow2["FirstName"] = "Sara";
            employeeDataRow2["LastName"] = "Gor";
            employeeDataRow2.AcceptChanges();
            employeeDataRow2.Delete();

            // 3. Add a row and modify its value (Modified state)
            IDataRow employeeDataRow3 = employeeTable.AddNewRow();
            employeeDataRow3["Id"] = 1;
            employeeDataRow3["FirstName"] = "Paul";
            employeeDataRow3["LastName"] = "Carry";
            employeeDataRow3.AcceptChanges();
            employeeDataRow3["FirstName"] = "Tom";

            // Act
            // 4. Reject changes at table level
            // - employeeDataRow1 is removed from the table because it was Added
            // - employeeDataRow2 remains in the table and becomes Unchanged
            // - employeeDataRow3 remains in the table and becomes Unchanged
            employeeTable.RejectChanges();

            // Assert
            Assert.Equal(2, employeeTable.Rows.Count);
            Assert.Equal(DataRowState.Unchanged, employeeDataRow2.DataRowState);
            Assert.Equal(DataRowState.Unchanged, employeeDataRow3.DataRowState);
            Assert.Equal("Paul", employeeDataRow3["FirstName"]);
        }
    }
}
