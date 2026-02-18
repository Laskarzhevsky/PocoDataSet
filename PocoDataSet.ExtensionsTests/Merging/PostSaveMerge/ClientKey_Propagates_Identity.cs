using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class PostSaveMerge
    {
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
