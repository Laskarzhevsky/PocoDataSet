using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void AddNewRowTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create an empty Department table
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // Act
            // 3. Add a new row to Department table
            IDataRow departmentDataRow = departmentDataTable.AddNewRow();

            // Assert
            Assert.Single(departmentDataTable.Rows);
            Assert.Equal(DataRowState.Added, departmentDataRow.DataRowState);
        }
    }
}
