using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void LocalAddedRow_NotDeleted()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            // Baseline row (loaded from server earlier)
            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            loaded["Id"] = 1;
            loaded["Name"] = "A";
            currentTable.AddLoadedRow(loaded);

            // Local client-added row (must not be removed by Refresh merge)
            IDataRow added = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            added["Id"] = 999;
            added["Name"] = "LOCAL";
            currentTable.AddRow(added);

            Assert.Equal(DataRowState.Added, added.DataRowState);

            // Refreshed snapshot contains only the baseline row
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

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
            // Baseline row remains, local Added row remains too
            Assert.Equal(2, currentTable.Rows.Count);

            bool hasId1 = false;
            bool hasId999 = false;

            for (int i = 0; i < currentTable.Rows.Count; i++)
            {
                IDataRow row = currentTable.Rows[i];
                if ((int)row["Id"]! == 1)
                {
                    hasId1 = true;
                }
                if ((int)row["Id"]! == 999)
                {
                    hasId999 = true;
                }
            }

            Assert.True(hasId1);
            Assert.True(hasId999);

            // Refresh should not report the local Added row as deleted
            Assert.Empty(result.DeletedDataRows);
        }
    }
}
