using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class PostSaveMerge
    {
        /// <summary>
        /// Verifies the *PostSave* contract for **deletions**.  Scenario: - The current dataset contains an existing
        /// row. - The server PostSave changeset contains a matching row marked as `Deleted`.  Expected behavior: - The
        /// merge identifies the target row (by PK / correlation key depending on schema). - The row is removed (or
        /// marked Deleted according to table semantics) so the current dataset reflects the server state after save. -
        /// The result proves that server-side deletes win during PostSave synchronization.
        /// </summary>

        [Fact]
        public void DeletesRow_WhenChangesetDeleted()
        {
            // Arrange
            // Current dataset with one baseline row (Id=2)
            IDataSet currentDataSet = DataSetFactory.CreateDataSet();
            IDataTable currentTable = currentDataSet.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            currentRow["Id"] = 2;
            currentRow["Name"] = "Financial";
            currentRow[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();

            // Baseline attach: Unchanged
            currentTable.AddLoadedRow(currentRow);

            Assert.Single(currentTable.Rows);
            Assert.Equal(DataRowState.Unchanged, currentTable.Rows[0].DataRowState);

            // Changeset dataset: contains the same row but marked as Deleted
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable changesetTable = changeset.AddNewTable("Department");
            changesetTable.AddColumn("Id", DataTypeNames.INT32);
            changesetTable.AddColumn("Name", DataTypeNames.STRING);
            changesetTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            IDataRow changesetRow = DataRowExtensions.CreateRowFromColumns(changesetTable.Columns);
            changesetRow["Id"] = 2;
            changesetRow["Name"] = "Financial";
            changesetRow[SpecialColumnNames.CLIENT_KEY] = currentRow[SpecialColumnNames.CLIENT_KEY];

            // IMPORTANT:
            // We must attach as baseline (Unchanged), then Delete().
            // Delete() cannot be called on Added rows by design.
            changesetTable.AddLoadedRow(changesetRow);

            Assert.Single(changesetTable.Rows);
            Assert.Equal(DataRowState.Unchanged, changesetTable.Rows[0].DataRowState);

            changesetRow.Delete();
            Assert.Equal(DataRowState.Deleted, changesetRow.DataRowState);

            // Act
            // Merge options are part of the contract surface; using defaults here exercises the standard behavior.
            MergeOptions options = new MergeOptions();
            // Execute PostSave merge: apply server-returned changes (Added/Modified/Deleted) onto current rows.
            currentDataSet.DoPostSaveMerge(changeset, options);

            // Assert
            // After PostSave merge, the row should be removed or deleted depending on your semantics.
            // Most PostSave implementations physically remove deleted rows after AcceptChanges.
            // We assert that the row no longer exists in the table.
            Assert.Empty(currentTable.Rows);
        }
    }
}
