using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void DuplicateCompositeKey_FirstWins()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("T");
            currentTable.AddColumn("A", DataTypeNames.INT32, false, true); // PK part 1
            currentTable.AddColumn("B", DataTypeNames.INT32, false, true); // PK part 2
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = currentTable.AddNewRow();
            currentRow["A"] = 1;
            currentRow["B"] = 2;
            currentRow["Name"] = "Current";

            current.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("T");
            refreshedTable.AddColumn("A", DataTypeNames.INT32, false, true);
            refreshedTable.AddColumn("B", DataTypeNames.INT32, false, true);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            // Duplicate composite key rows in refreshed snapshot.
            IDataRow first = refreshedTable.AddNewRow();
            first["A"] = 1;
            first["B"] = 2;
            first["Name"] = "First";

            IDataRow second = refreshedTable.AddNewRow();
            second["A"] = 1;
            second["B"] = 2;
            second["Name"] = "Second";

            MergeOptions mergeOptions = new MergeOptions();

            // Act (should not throw; PK index is "first wins")
            current.DoRefreshMergePreservingLocalChanges(refreshed, mergeOptions);

            // Assert: current row matches the first refreshed row (historic behavior).
            Assert.Equal("First", (string)currentTable.Rows[0]["Name"]!);
        }
    }
}
