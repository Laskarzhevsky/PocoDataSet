using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class PostSaveMerge
    {
        [Fact]
        public void ClientKey_Propagates_ServerIdentity()
        {
            // Arrange
            // current dataset has a client-created row without server identity yet.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

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

            IDataRow serverRow = DataRowExtensions.CreateRowFromColumns(changesetTable.Columns);
            serverRow["Id"] = 123;
            serverRow["Name"] = "Customer Service";
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;

            changesetTable.AddRow(serverRow); // Added in changeset

            // Act
            MergeOptions options = new MergeOptions();
            // Execute PostSave merge: apply server-returned changes (Added/Modified/Deleted) onto current rows.
            current.DoPostSaveMerge(changeset, options);

            // Assert
            // identity propagated, row accepted
            Assert.Single(currentTable.Rows);

            IDataRow merged = currentTable.Rows[0];
            Assert.Equal(123, merged["Id"]);
            Assert.Equal("Customer Service", merged["Name"]);
            Assert.Equal(clientKey, merged[SpecialColumnNames.CLIENT_KEY]);

            Assert.Equal(DataRowState.Unchanged, merged.DataRowState);
            Assert.False(merged.HasOriginalValues);
        }
    }
}
