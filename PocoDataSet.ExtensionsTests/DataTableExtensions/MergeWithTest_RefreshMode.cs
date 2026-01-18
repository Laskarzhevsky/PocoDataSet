using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void MergeWithTest_RefreshMode()
        {
            // Arrange
            // 1. Create current table (for example, UI-bound)
            IDataSet currentDataSet = DataSetFactory.CreateDataSet();
            IDataTable currentDepartment = currentDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = currentDepartment.AddNewRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Sales";
            currentRow.AcceptChanges();

            // 2. Create refreshed table snapshot
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartment = refreshedDataSet.AddNewTable("Department");
            refreshedDepartment.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Sales and Marketing";

            // Act
            // 3. Merge refreshed values into current table using Refresh mode
            IMergeOptions options = new MergeOptions();
            options.MergeMode = MergeMode.Refresh;
            currentDepartment.MergeWith(refreshedDepartment, options);

            // Assert
            // 4. Current row now has refreshed values
            string? name = currentDepartment.GetFieldValue<string>(0, "Name");

            Assert.Equal("Sales and Marketing", name);
        }
    }
}
