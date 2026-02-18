using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies that locally Added rows remain present after *RefreshPreservingLocalChanges* even if the server
        /// snapshot does not include them.  This is the "Added rows are authoritative locally until saved" guarantee.
        /// Expected behavior: - The Added row remains in the table. - The row state remains Added and is not converted
        /// to Deleted/Unchanged.
        /// </summary>

        [Fact]
        public void PreservesAddedRow_WhenMissingFromSnapshot()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow added = t.AddNewRow();
            added["Id"] = 100;
            added["Name"] = "ClientOnly";

            Assert.Equal(DataRowState.Added, added.DataRowState);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);

            // Act
            // Merge options are part of the contract surface; using defaults here exercises the standard behavior.
            MergeOptions options = new MergeOptions();
            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            // Added row preserved
            Assert.Equal(1, t.Rows.Count);
            Assert.Equal("ClientOnly", t.Rows[0]["Name"]);
            Assert.Equal(DataRowState.Added, t.Rows[0].DataRowState);
        }
    }
}
