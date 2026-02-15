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

            IDataRow refreshedRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(table.Columns);
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Server";
            refreshedRow.AcceptChanges();

            IMergeOptions options = new MergeOptions();

            // Act
            bool changed = currentRow.DoRefreshMergePreservingLocalChanges(refreshedRow, table.TableName, table.Columns, options);

            // Assert
            Assert.False(changed);
            Assert.Equal(DataRowState.Deleted, currentRow.DataRowState);
        }
    }
}
