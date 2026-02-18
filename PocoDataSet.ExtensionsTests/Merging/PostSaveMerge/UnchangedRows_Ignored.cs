using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the contract that PostSave merge processes only Added/Modified/Deleted rows from the
    /// changeset snapshot. Unchanged rows in the changeset must be ignored.
    ///
    /// Scenario:
    /// - Build a CURRENT DataSet (the client-side truth before the merge).
    /// - Build a REFRESHED/CHANGESET DataSet (the server-side snapshot or post-save response).
    /// - Execute PostSave merge (apply server post-save changeset onto current).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
    /// Notes:
    /// - This file contains exactly one test method: UnchangedRows_Ignored.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void UnchangedRows_Ignored()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow baseline = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            baseline["Id"] = 10;
            baseline["Name"] = "Before";
            currentTable.AddLoadedRow(baseline);

            // Create a local pending change
            baseline["Name"] = "Edited Locally";
            Assert.Equal(DataRowState.Modified, baseline.DataRowState);
            Assert.True(baseline.HasOriginalValues);

            // Changeset row with the same PK, but kept Unchanged on purpose.
            // PostSave must ignore it (no AcceptChanges, no overwrite).
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable changesetTable = changeset.AddNewTable("Department");
            changesetTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            changesetTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow serverRowUnchanged = DataRowExtensions.CreateRowFromColumns(changesetTable.Columns);
            serverRowUnchanged["Id"] = 10;
            serverRowUnchanged["Name"] = "Server Value That Must Be Ignored";
            changesetTable.AddLoadedRow(serverRowUnchanged);
            Assert.Equal(DataRowState.Unchanged, serverRowUnchanged.DataRowState);

            // Act
            MergeOptions options = new MergeOptions();
            // Execute PostSave merge: apply server-returned changes (Added/Modified/Deleted) onto current rows.
            current.DoPostSaveMerge(changeset, options);

            // Assert
            Assert.Single(currentTable.Rows);
            Assert.Equal("Edited Locally", (string)baseline["Name"]!);
            Assert.Equal(DataRowState.Modified, baseline.DataRowState);
            Assert.True(baseline.HasOriginalValues);
        }
    }
}
