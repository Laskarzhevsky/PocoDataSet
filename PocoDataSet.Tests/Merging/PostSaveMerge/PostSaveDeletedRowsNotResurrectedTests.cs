using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// Locks the PostSave contract around current Deleted rows.
    ///
    /// Current behavior: if the changeset contains a row matching a current Deleted row by PK,
    /// PostSave attempts to merge and calls AcceptChanges at row level, which is not allowed for Deleted rows.
    /// The merge throws and the row remains Deleted (it is not resurrected).
    /// </summary>
    public class PostSaveDeletedRowsNotResurrectedTests
    {
        [Fact]
        public void PostSave_Throws_AndDoesNotResurrect_WhenChangesetContainsRowMatchingCurrentDeletedRow_ByPk()
        {
            // ------------------------------------------------------------
            // Arrange
            // ------------------------------------------------------------
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

            // ------------------------------------------------------------
            // Act
            // ------------------------------------------------------------
            MergeOptions options = new MergeOptions();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                delegate
                {
                    current.DoPostSaveMerge(changeset, options);
                });

            // ------------------------------------------------------------
            // Assert
            // ------------------------------------------------------------
            Assert.Contains("AcceptChanges for Deleted rows must be performed at table level", ex.Message);

            // The deleted row remains deleted and is not resurrected.
            Assert.Single(currentTable.Rows);
            Assert.Equal(DataRowState.Deleted, currentTable.Rows[0].DataRowState);
        }
    }
}
