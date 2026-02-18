using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks PostSave **result accounting** across a mixed changeset: Insert (Added), Update (Modified), Delete.
    ///
    /// Scenario:
    /// - CURRENT has:
    ///   (1) an Added row (new insert, correlated by __ClientKey)
    ///   (2) an existing row that is locally Modified
    ///   (3) an existing row that should be deleted by the server
    /// - SERVER PostSave changeset returns:
    ///   (1) Added row confirming insert (same __ClientKey, server may assign final values)
    ///   (2) Modified row confirming update (same PK)
    ///   (3) Deleted row confirming delete (same PK)
    ///
    /// Expected behavior:
    /// - PostSave applies the changes and AcceptChanges so (1) and (2) end Unchanged.
    /// - The deleted row is removed from the current table.
    /// - MergeResult records:
    ///   - 2 UpdatedDataRows entries (because both the Added and Modified rows merge into existing target rows),
    ///   - 1 DeletedDataRows entry.
    ///
    /// How this test proves it:
    /// - Arrange sets up current state with Added/Modified baseline and builds a matching changeset.
    /// - Act merges and reads <see cref="IDataSetMergeResult"/>.
    /// - Assert validates final row states and the exact merge result counts, and ensures each row is referenced
    ///   by the merge result (using the shared helper <c>ContainsRow</c>).
    ///
    /// Notes:
    /// - This file contains exactly one test method: MergeResult_Tracks_AllOperations.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void MergeResult_Tracks_AllOperations()
        {
            // Arrange (CURRENT)
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn("RowVersion", DataTypeNames.BINARY);

            // (1) Added row (insert)
            Guid insertClientKey = Guid.NewGuid();

            IDataRow addedRow = new DataRow();
            addedRow[SpecialColumnNames.CLIENT_KEY] = insertClientKey;
            addedRow["Id"] = -1; // client temp
            addedRow["Name"] = "New Dept";
            addedRow["RowVersion"] = new byte[] { 1 };
            t.AddRow(addedRow);

            Assert.Equal(DataRowState.Added, addedRow.DataRowState);

            // (2) Modified row (update)
            Guid updateClientKey = Guid.NewGuid();

            IDataRow modifiedRow = DataRowExtensions.CreateRowFromColumns(t.Columns);
            modifiedRow[SpecialColumnNames.CLIENT_KEY] = updateClientKey;
            modifiedRow["Id"] = 20;
            modifiedRow["Name"] = "Before";
            modifiedRow["RowVersion"] = new byte[] { 2 };
            t.AddLoadedRow(modifiedRow);

            // Local edit (Modified)
            modifiedRow["Name"] = "Edited Locally";
            Assert.Equal(DataRowState.Modified, modifiedRow.DataRowState);

            // (3) Row that will be deleted
            Guid deleteClientKey = Guid.NewGuid();

            IDataRow deleteRow = DataRowExtensions.CreateRowFromColumns(t.Columns);
            deleteRow[SpecialColumnNames.CLIENT_KEY] = deleteClientKey;
            deleteRow["Id"] = 30;
            deleteRow["Name"] = "To Delete";
            deleteRow["RowVersion"] = new byte[] { 3 };
            t.AddLoadedRow(deleteRow);

            Assert.Equal(DataRowState.Unchanged, deleteRow.DataRowState);

            // Arrange (SERVER CHANGESET)
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable ct = changeset.AddNewTable("Department");
            ct.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);
            ct.AddColumn("RowVersion", DataTypeNames.BINARY);

            // (1) Insert confirmation (Added)
            IDataRow serverInsert = new DataRow();
            serverInsert[SpecialColumnNames.CLIENT_KEY] = insertClientKey;
            serverInsert["Id"] = 10; // server assigns final PK
            serverInsert["Name"] = "New Dept";
            serverInsert["RowVersion"] = new byte[] { 9 };
            ct.AddRow(serverInsert);
            serverInsert.SetDataRowState(DataRowState.Added);

            // (2) Update confirmation (Modified)
            IDataRow serverUpdate = DataRowExtensions.CreateRowFromColumns(ct.Columns);
            serverUpdate[SpecialColumnNames.CLIENT_KEY] = updateClientKey;
            serverUpdate["Id"] = 20;
            serverUpdate["Name"] = "Edited Locally";
            serverUpdate["RowVersion"] = new byte[] { 8 };
            ct.AddLoadedRow(serverUpdate);
            serverUpdate.AcceptChanges();
            // Change a value after AcceptChanges() so this SERVER row is truly Modified (not Unchanged).
            serverUpdate["Name"] = "Edited On Server";
            Assert.Equal(DataRowState.Modified, serverUpdate.DataRowState);

            // (3) Delete confirmation (Deleted)
            IDataRow serverDelete = DataRowExtensions.CreateRowFromColumns(ct.Columns);
            serverDelete[SpecialColumnNames.CLIENT_KEY] = deleteClientKey;
            serverDelete["Id"] = 30;
            serverDelete["Name"] = "To Delete";
            serverDelete["RowVersion"] = new byte[] { 7 };
            ct.AddLoadedRow(serverDelete);
            serverDelete.Delete();
            Assert.Equal(DataRowState.Deleted, serverDelete.DataRowState);

            MergeOptions options = new MergeOptions();
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Act
            current.DoPostSaveMerge(changeset, options);

            // Assert (final data shape)
            // One row was deleted -> the table should now contain 2 rows.
            Assert.Equal(2, t.Rows.Count);

            // Added row is now accepted and has server PK + concurrency token.
            Assert.Equal(10, (int)addedRow["Id"]!);
            Assert.Equal(DataRowState.Unchanged, addedRow.DataRowState);
            Assert.Equal(9, ((byte[])addedRow["RowVersion"]!)[0]);

            // Modified row accepted and has updated concurrency token.
            Assert.Equal(DataRowState.Unchanged, modifiedRow.DataRowState);
            Assert.Equal(8, ((byte[])modifiedRow["RowVersion"]!)[0]);

            // Deleted row removed from the table.
            for (int i = 0; i < t.Rows.Count; i++)
            {
                Assert.NotSame(deleteRow, t.Rows[i]);
            }

            // Assert (merge result accounting)
            Assert.Equal(0, result.AddedDataRows.Count);
            Assert.Equal(2, result.UpdatedDataRows.Count);
            Assert.Equal(1, result.DeletedDataRows.Count);

            // Each row must be referenced by the merge result.
            Assert.True(ContainsRow(result, "Department", addedRow));
            Assert.True(ContainsRow(result, "Department", modifiedRow));
            Assert.True(ContainsRow(result, "Department", deleteRow));
        }
    }
}