using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class PostSaveMerge
    {
        /// <summary>
        /// Verifies the *PostSave* contract for **rowversion / concurrency tokens**.  Scenario: - The client has a row
        /// with an old rowversion value. - The server PostSave changeset returns the same row with an updated
        /// rowversion.  Expected behavior: - PostSave merge updates the concurrency token on the current row to the
        /// server value. - The row ends `Unchanged` (it now represents the saved server snapshot). - This locks the
        /// invariant that clients continue to use the latest server-issued concurrency token for the next update.
        /// </summary>

        [Fact]
        public void RowVersion_Propagated()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentTable.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid clientKey = Guid.NewGuid();

            IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            currentRow["Id"] = 10;
            currentRow["Name"] = "Before";
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentRow["RowVersion"] = new byte[] { 1, 2, 3, 4 };
            currentTable.AddLoadedRow(currentRow);

            // Simulate a local edit before save
            currentRow["Name"] = "Edited";
            Assert.Equal(DataRowState.Modified, currentRow.DataRowState);

            // Changeset returned from server after save:
            // same logical row, with updated RowVersion (and server-confirmed values)
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable changesetTable = changeset.AddNewTable("Department");
            changesetTable.AddColumn("Id", DataTypeNames.INT32);
            changesetTable.AddColumn("Name", DataTypeNames.STRING);
            changesetTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            changesetTable.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow serverRow = DataRowExtensions.CreateRowFromColumns(changesetTable.Columns);
            serverRow["Id"] = 10;
            serverRow["Name"] = "Edited";
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow["RowVersion"] = new byte[] { 9, 9, 9, 9 };

            // Mark as Modified in the changeset so PostSave applies it as an update
            serverRow.AcceptChanges();
            serverRow["Name"] = "Edited";
            changesetTable.AddRow(serverRow);

            // Act
            // Merge options are part of the contract surface; using defaults here exercises the standard behavior.
            MergeOptions options = new MergeOptions();
            // Execute PostSave merge: apply server-returned changes (Added/Modified/Deleted) onto current rows.
            current.DoPostSaveMerge(changeset, options);

            // Assert
            Assert.Equal("Edited", currentRow["Name"]);
            Assert.Equal(DataRowState.Unchanged, currentRow.DataRowState);

            byte[] rv = (byte[])currentRow["RowVersion"]!;
            Assert.Equal(4, rv.Length);
            Assert.Equal((byte)9, rv[0]);
            Assert.Equal((byte)9, rv[1]);
            Assert.Equal((byte)9, rv[2]);
            Assert.Equal((byte)9, rv[3]);
        }
    }
}
