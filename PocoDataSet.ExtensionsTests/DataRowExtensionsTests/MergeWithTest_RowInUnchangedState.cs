using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataRowExtensionsTests
    {
        [Fact]
        public void DoRefreshMergePreservingLocalChanges_RowInUnchangedState_IsUpdatedAndAccepted()
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

            IDataRow refreshedRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(table.Columns);
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "New";
            refreshedRow.AcceptChanges();

            IMergeOptions options = new MergeOptions();

            // Act
            bool changed = currentRow.DoRefreshMergePreservingLocalChanges(refreshedRow, table.TableName, table.Columns, options);

            // Assert
            Assert.True(changed);
            Assert.Equal("New", currentRow.GetDataFieldValue<string>("Name"));
            Assert.Equal(DataRowState.Unchanged, currentRow.DataRowState);
        }
    }
}
