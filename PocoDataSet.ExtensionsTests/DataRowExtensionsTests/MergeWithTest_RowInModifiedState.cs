using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void MergeWith_RowInModifiedState()
        {
            // 1. Create a new  data set to simulate UI bound data source
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Create  table and row
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow departmentDataRow = departmentDataTable.AddNewRow();
            departmentDataRow["Id"] = 1;
            departmentDataRow["Name"] = "Sales";

            // 3. Accept the row changes and modify its data to put row into Modified state
            departmentDataRow.AcceptChanges();
            departmentDataRow["Name"] = "Reception";

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
            // 5. Call MergeWith method and observe that merge is not done because the  row in Modified state
            departmentDataRow.MergeWith(departmentRefreshedDataRow, "Department", departmentDataTable.Columns, new MergeOptions());

            // Assert
            Assert.Equal(departmentDataRow.DataRowState.ToString(), DataRowState.Modified.ToString());
            Assert.Equal("Reception", departmentDataRow["Name"]);
        }
    }
}
