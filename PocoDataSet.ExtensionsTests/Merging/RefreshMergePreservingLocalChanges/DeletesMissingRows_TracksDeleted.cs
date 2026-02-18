using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void DeletesMissingRows_TracksDeleted()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            r1["Id"] = 1;
            r1["Name"] = "A";
            currentTable.AddLoadedRow(r1);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            r2["Id"] = 2;
            r2["Name"] = "B";
            currentTable.AddLoadedRow(r2);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            // refreshed contains only Id=1; Id=2 is missing -> should be treated as deleted in refresh merge
            IDataRow rr1 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            rr1["Id"] = 1;
            rr1["Name"] = "A";
            refreshedTable.AddLoadedRow(rr1);

            // Act
            MergeOptions options = new MergeOptions();
            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Assert
            // Refresh merge treats refreshed data as authoritative snapshot,
            // so missing rows are removed from the current table.
            Assert.Single(currentTable.Rows);
            Assert.Equal(1, currentTable.Rows[0]["Id"]);
            Assert.Equal("A", currentTable.Rows[0]["Name"]);

            // But merge result should still report that a row was deleted (Id=2).
            Assert.Single(result.DeletedDataRows);
        }
    }
}
