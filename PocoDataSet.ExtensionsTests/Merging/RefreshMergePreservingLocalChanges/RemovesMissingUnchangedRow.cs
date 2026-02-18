using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void RemovesMissingUnchangedRow()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One";
            currentTable.AddLoadedRow(r1);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            r2["Id"] = 2;
            r2["Name"] = "Two";
            currentTable.AddLoadedRow(r2);

            IDataRow r3 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            r3["Id"] = 3;
            r3["Name"] = "Three";
            currentTable.AddLoadedRow(r3);

            // Refreshed snapshot is missing Id=2
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            rr1["Id"] = 1;
            rr1["Name"] = "One";
            refreshedTable.AddLoadedRow(rr1);

            IDataRow rr3 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            rr3["Id"] = 3;
            rr3["Name"] = "Three";
            refreshedTable.AddLoadedRow(rr3);

            // Act
            MergeOptions options = new MergeOptions();
            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Assert
            // row 2 must be removed (or counted as deleted in merge result)
            Assert.Equal(2, currentTable.Rows.Count);

            bool has1 = false;
            bool has2 = false;
            bool has3 = false;

            for (int i = 0; i < currentTable.Rows.Count; i++)
            {
                IDataRow row = currentTable.Rows[i];
                int id = (int)row["Id"]!;
                if (id == 1)
                {
                    has1 = true;
                }
                if (id == 2)
                {
                    has2 = true;
                }
                if (id == 3)
                {
                    has3 = true;
                }
            }

            Assert.True(has1);
            Assert.False(has2);
            Assert.True(has3);

            Assert.Single(result.DeletedDataRows);
        }
    }
}
