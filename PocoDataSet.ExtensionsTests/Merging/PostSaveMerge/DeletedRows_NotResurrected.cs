using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the PostSave contract around current Deleted rows. Current behavior: if the changeset
    /// contains a row matching a current Deleted row by PK, PostSave attempts to merge and calls
    /// AcceptChanges at row level, which is not allowed for Deleted rows. The merge throws and the
    /// row remains Deleted (it is not resurrected).
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
    /// - This file contains exactly one test method: DeletedRows_NotResurrected.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void DeletedRows_NotResurrected()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            row["Id"] = 10;
            row["Name"] = "To Be Deleted";
            currentTable.AddLoadedRow(row);

            row.Delete();
            Assert.Equal(DataRowState.Deleted, row.DataRowState);

            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable changesetTable = changeset.AddNewTable("Department");
            changesetTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            changesetTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow serverRow = changesetTable.AddNewRow();
            serverRow["Id"] = 10;
            serverRow["Name"] = "Server Says Exists";
            Assert.Equal(DataRowState.Added, serverRow.DataRowState);

            // Act
            MergeOptions options = new MergeOptions();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                delegate
                {
            // Execute PostSave merge: apply server-returned changes (Added/Modified/Deleted) onto current rows.
                    current.DoPostSaveMerge(changeset, options);
                });

            // Assert
            Assert.Contains("AcceptChanges for Deleted rows must be performed at table level", ex.Message);

            // The deleted row remains deleted and is not resurrected.
            Assert.Single(currentTable.Rows);
            Assert.Equal(DataRowState.Deleted, currentTable.Rows[0].DataRowState);
        }
    }
}
