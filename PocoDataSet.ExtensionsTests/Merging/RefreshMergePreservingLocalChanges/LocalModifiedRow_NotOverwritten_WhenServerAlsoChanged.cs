using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies conflict handling in *RefreshPreservingLocalChanges* when **both client and server changed the same
        /// row**.  Scenario: - Current row is Modified locally. - Refreshed snapshot contains different values for the
        /// same PK (server also changed).  Expected behavior: - Local modifications are preserved (the current values
        /// are not overwritten). - The merge may still update non-conflicting metadata (depending on design), but the
        /// key guarantee is: do not lose local edits.
        /// </summary>

        [Fact]
        public void LocalModifiedRow_NotOverwritten_WhenServerAlsoChanged()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 1;
            row["Name"] = "ServerOld";
            t.AddLoadedRow(row);

            // Local edit
            row["Name"] = "LocalEdit";
            Assert.Equal(DataRowState.Modified, row.DataRowState);

            // Refreshed snapshot also changed the same row
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "ServerNew";
            rt.AddLoadedRow(r1);

            // Act
            // Merge options are part of the contract surface; using defaults here exercises the standard behavior.
            MergeOptions options = new MergeOptions();
            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            // local edit is preserved
            Assert.Equal("LocalEdit", row["Name"]);
            Assert.Equal(DataRowState.Modified, row.DataRowState);
        }
    }
}
