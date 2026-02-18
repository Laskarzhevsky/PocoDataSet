using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void ColumnAddedAfterRowsExist_RowMissingKey_DoesNotThrow()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 1;
            row["Name"] = "Old";
            t.AddLoadedRow(row);

            // Add a column after rows already exist (row may not contain this key internally)
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumns(t.Columns);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "New";
            r1[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();
            rt.AddLoadedRow(r1);

            // Act + Assert: must not throw
            MergeOptions options = new MergeOptions();
            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // And Name must be updated (row is Unchanged during refresh)
            Assert.Equal("New", row["Name"]);
            Assert.Equal(DataRowState.Unchanged, row.DataRowState);
        }
    }
}
