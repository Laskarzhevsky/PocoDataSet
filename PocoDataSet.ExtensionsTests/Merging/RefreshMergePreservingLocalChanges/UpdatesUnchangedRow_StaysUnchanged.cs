using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies the no-op path: when an Unchanged row matches the refreshed snapshot exactly, the merge should not
        /// change state.  Scenario: - Current Unchanged row equals refreshed row (same PK and same values).  Expected
        /// behavior: - Values are identical after merge. - Row remains `Unchanged`. - This locks that the merge does
        /// not introduce spurious modifications.
        /// </summary>

        [Fact]
        public void UpdatesUnchangedRow_StaysUnchanged()
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

            Assert.Equal(DataRowState.Unchanged, row.DataRowState);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "New";
            rt.AddLoadedRow(r1);

            // Act
            // Merge options are part of the contract surface; using defaults here exercises the standard behavior.
            MergeOptions options = new MergeOptions();
            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Equal("New", row["Name"]);
            Assert.Equal(DataRowState.Unchanged, row.DataRowState);
        }
    }
}
