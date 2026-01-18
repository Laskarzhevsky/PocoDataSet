using System;
using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class PostSaveMergeTests
    {
        [Fact]
        public void Merge_PostSave_PropagatesServerIdentity_ByClientKey_AndAcceptsChanges()
        {
            // Arrange: current dataset has a client-created row without server identity yet.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            currentTable.PrimaryKeys = new List<string> { "Id" };

            Guid clientKey = Guid.NewGuid();

            IDataRow uiRow = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            uiRow["Id"] = 0; // not assigned yet
            uiRow["Name"] = "Customer Service";
            uiRow[SpecialColumnNames.CLIENT_KEY] = clientKey;

            currentTable.AddRow(uiRow);
            Assert.Equal(DataRowState.Added, uiRow.DataRowState);

            // Arrange: changeset returned from server contains the same client key and server identity.
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable changesetTable = changeset.AddNewTable("Department");
            changesetTable.AddColumn("Id", DataTypeNames.INT32);
            changesetTable.AddColumn("Name", DataTypeNames.STRING);
            changesetTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            changesetTable.PrimaryKeys = new List<string> { "Id" };

            IDataRow serverRow = DataRowExtensions.CreateRowFromColumns(changesetTable.Columns);
            serverRow["Id"] = 123;
            serverRow["Name"] = "Customer Service";
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;

            changesetTable.AddRow(serverRow); // Added in changeset

            // Act
            current.MergeWith(changeset, MergeMode.PostSave);

            // Assert: identity propagated, row accepted
            Assert.Single(currentTable.Rows);

            IDataRow merged = currentTable.Rows[0];
            Assert.Equal(123, merged["Id"]);
            Assert.Equal("Customer Service", merged["Name"]);
            Assert.Equal(clientKey, merged[SpecialColumnNames.CLIENT_KEY]);

            Assert.Equal(DataRowState.Unchanged, merged.DataRowState);
            Assert.False(merged.HasOriginalValues);
        }

        [Fact]
        public void Merge_PostSave_DeletesRow_WhenChangesetMarksDeleted()
        {
            // ------------------------------------------------------------
            // Arrange
            // ------------------------------------------------------------

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

            // ------------------------------------------------------------
            // Act
            // ------------------------------------------------------------
            currentDataSet.MergeWith(changeset, MergeMode.PostSave);

            // ------------------------------------------------------------
            // Assert
            // ------------------------------------------------------------

            // After PostSave merge, the row should be removed or deleted depending on your semantics.
            // Most PostSave implementations physically remove deleted rows after AcceptChanges.
            // We assert that the row no longer exists in the table.
            Assert.Empty(currentTable.Rows);
        }

        [Fact]
        public void Merge_PostSave_PropagatesRowVersion_AndAcceptsChanges()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentTable.AddColumn("RowVersion", DataTypeNames.BINARY);
            currentTable.PrimaryKeys = new List<string> { "Id" };

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
            changesetTable.PrimaryKeys = new List<string> { "Id" };

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
            current.MergeWith(changeset, MergeMode.PostSave);

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

        [Fact]
        public void Merge_PostSave_PropagatesIdentity_ByClientKey_AndAcceptsChanges()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentTable.PrimaryKeys = new List<string> { "Id" };

            Guid clientKey = Guid.NewGuid();

            // Local newly-added row before save: Id not assigned yet (0), state Added
            IDataRow local = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            local["Id"] = 0;
            local["Name"] = "NewDept";
            local[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentTable.AddRow(local);

            Assert.Equal(DataRowState.Added, local.DataRowState);

            // Server returns saved row: same __ClientKey, assigned identity Id=123
            IDataSet serverChangeset = DataSetFactory.CreateDataSet();
            IDataTable serverTable = serverChangeset.AddNewTable("Department");
            serverTable.AddColumn("Id", DataTypeNames.INT32);
            serverTable.AddColumn("Name", DataTypeNames.STRING);
            serverTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            serverTable.PrimaryKeys = new List<string> { "Id" };

            IDataRow saved = DataRowExtensions.CreateRowFromColumns(serverTable.Columns);
            saved["Id"] = 123;
            saved["Name"] = "NewDept";
            saved[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverTable.AddRow(saved); // Keep as Added in changeset

            // Act
            current.MergeWith(serverChangeset, MergeMode.PostSave);

            // Assert: local row gets identity, becomes Unchanged
            Assert.Equal(123, (int)local["Id"]!);
            Assert.Equal("NewDept", local["Name"]);
            Assert.Equal(DataRowState.Unchanged, local.DataRowState);
        }
    }
}
