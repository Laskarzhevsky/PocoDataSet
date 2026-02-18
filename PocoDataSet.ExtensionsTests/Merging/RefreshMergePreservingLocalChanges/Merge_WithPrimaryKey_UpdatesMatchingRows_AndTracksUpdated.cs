using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void Merge_WithPrimaryKey_UpdatesMatchingRows_AndTracksUpdated()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            currentRow["Id"] = 2;
            currentRow["Name"] = "Financial";
            currentTable.AddLoadedRow(currentRow);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            refreshedRow["Id"] = 2;
            refreshedRow["Name"] = "Reception";
            refreshedTable.AddLoadedRow(refreshedRow);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Assert
            Assert.Single(currentTable.Rows);
            Assert.Equal(2, currentTable.Rows[0]["Id"]);
            Assert.Equal("Reception", currentTable.Rows[0]["Name"]);

            // Refresh merge treats refreshed data as baseline
            Assert.Equal(DataRowState.Unchanged, currentTable.Rows[0].DataRowState);

            // The merge result should record an update
            Assert.Single(result.UpdatedDataRows);
            Assert.Empty(result.AddedDataRows);
            Assert.Empty(result.DeletedDataRows);
        }
    }
}
