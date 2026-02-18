using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void NoPrimaryKeys_ReplacesAllRows_AndTracksAddedDeleted()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("NoPkTable");
            currentTable.AddColumn("Code", DataTypeNames.STRING);
            currentTable.AddColumn("Value", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            c1["Code"] = "A";
            c1["Value"] = "One";
            currentTable.AddLoadedRow(c1);

            IDataRow c2 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            c2["Code"] = "B";
            c2["Value"] = "Two";
            currentTable.AddLoadedRow(c2);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("NoPkTable");
            refreshedTable.AddColumn("Code", DataTypeNames.STRING);
            refreshedTable.AddColumn("Value", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            r1["Code"] = "C";
            r1["Value"] = "Three";
            refreshedTable.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();

            // Ensure merge result starts empty
            options.DataSetMergeResult.Clear();

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Equal(1, currentTable.Rows.Count);
            Assert.Equal("C", currentTable.Rows[0]["Code"]);
            Assert.Equal("Three", currentTable.Rows[0]["Value"]);

            Assert.Equal(2, options.DataSetMergeResult.DeletedDataRows.Count);
            Assert.Equal(1, options.DataSetMergeResult.AddedDataRows.Count);

            // Replace-all adds refreshed rows as new rows
            Assert.Equal(DataRowState.Unchanged, currentTable.Rows[0].DataRowState);
        }
    }
}
