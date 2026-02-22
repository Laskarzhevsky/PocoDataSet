using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void DoRefreshMergePreservingLocalChanges_RowInDeletedState_IsNotOverwritten()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = table.AddNewRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Old";
            currentRow.AcceptChanges();

            currentRow.Delete();

            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshedDataSet.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = refreshedTable.AddNewRow();
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Server";
            refreshedRow.AcceptChanges();

            IMergeOptions options = new MergeOptions();
            // Act
            dataSet.DoRefreshMergePreservingLocalChanges(refreshedDataSet, options);

            // Assert            Assert.Equal(DataRowState.Deleted, currentRow.DataRowState);
        }
    }
}