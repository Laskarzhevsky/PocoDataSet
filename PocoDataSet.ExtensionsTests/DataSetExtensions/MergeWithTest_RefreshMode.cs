using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void MergeWithTest_RefreshMode()
        {
            // Arrange
            // 1. Create a current data set (for example, UI-bound)
            IDataSet currentDataSet = DataSetFactory.CreateDataSet();
            IDataTable currentDepartment = currentDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = currentDepartment.AddNewRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Sales";
            currentRow.AcceptChanges(); // put row into Unchanged state

            // 2. Create a refreshed data set returned from database / service
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartment = refreshedDataSet.AddNewTable("Department");
            refreshedDepartment.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Sales and Marketing";

            // Act
            // 3. Merge refreshed values into current data set using Refresh mode
            IDataSetMergeResult mergeResult = currentDataSet.MergeWith(refreshedDataSet, MergeMode.RefreshPreservingLocalChanges);

            // Assert
            // 4. Current row now has refreshed values "Sales and Marketing"
            string? name = currentDepartment.Rows[0].GetDataFieldValue<string>("Name");
            Assert.Equal("Sales and Marketing", name);
        }
    }
}
