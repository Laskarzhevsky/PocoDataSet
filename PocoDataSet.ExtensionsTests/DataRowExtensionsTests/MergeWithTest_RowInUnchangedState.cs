using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void MergeWith_RowInUnchangedState()
        {
            // Arrange
            // 1. Create a new  data set to simulate UI bound data source
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create  table and row
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow departmentDataRow = departmentDataTable.AddNewRow();
            departmentDataRow["Id"] = 1;
            departmentDataRow["Name"] = "Sales";

            // 3. Accept  row changes to put row into Unchanged state
            departmentDataRow.AcceptChanges();

            // 4. Create a new data set simulating refreshed data came form database
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();

            IDataTable refreshedDepartmentDataTable = refreshedDataSet.AddNewTable("Department");
            refreshedDepartmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow departmentRefreshedDataRow = refreshedDepartmentDataTable.AddNewRow();
            departmentRefreshedDataRow["Id"] = 1;
            departmentRefreshedDataRow["Name"] = "Financial";
            departmentRefreshedDataRow.AcceptChanges();

            // Act
            // 5. Call MergeWith method and observe that merge is completed because the  row was in Unchanged state
            departmentDataRow.MergeWith(departmentRefreshedDataRow, "Department", departmentDataTable.Columns, new MergeOptions());

            // Assert
            Assert.Equal(DataRowState.Unchanged.ToString(), departmentDataRow.DataRowState.ToString());
            Assert.Equal("Financial", departmentDataRow["Name"]);
        }
    }
}
