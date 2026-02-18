using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class PostSaveMerge
    {
        /// <summary>
        /// Verifies the *PostSave* contract for **server-assigned identity values** when correlating an Added row via
        /// `__ClientKey`.  Scenario: - The client has a newly Added row with `Id = 0` (identity not assigned yet) and a
        /// stable `__ClientKey`. - The server returns a PostSave changeset containing the same logical row (same
        /// `__ClientKey`) but with `Id` assigned.  Expected behavior: - The merge matches rows by `__ClientKey` (not by
        /// `Id`, which is not reliable yet). - The current row receives the server-assigned `Id`. - The row transitions
        /// to `Unchanged` (it is now in-sync with the server snapshot).
        /// </summary>

        [Fact]
        public void ClientKey_Propagates_Identity()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

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

            IDataRow saved = DataRowExtensions.CreateRowFromColumns(serverTable.Columns);
            saved["Id"] = 123;
            saved["Name"] = "NewDept";
            saved[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverTable.AddRow(saved); // Keep as Added in changeset

            // Act
            // Merge options are part of the contract surface; using defaults here exercises the standard behavior.
            MergeOptions options = new MergeOptions();
            // Execute PostSave merge: apply server-returned changes (Added/Modified/Deleted) onto current rows.
            current.DoPostSaveMerge(serverChangeset, options);

            // Assert
            // local row gets identity, becomes Unchanged
            Assert.Equal(123, (int)local["Id"]!);
            Assert.Equal("NewDept", local["Name"]);
            Assert.Equal(DataRowState.Unchanged, local.DataRowState);
        }
    }
}
