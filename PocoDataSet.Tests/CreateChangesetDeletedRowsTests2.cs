using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public partial class CreateChangesetDeletedRowsTests2
    {
        [Fact]
        public void CreateChangeset_IncludesDeletedRow_AndContainsPrimaryKeyValue()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            table.PrimaryKeys = new List<string> { "Id" };

            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(table.Columns);
            loaded["Id"] = 10;
            loaded["Name"] = "HR";
            loaded[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();

            table.AddLoadedRow(loaded);

            // Delete row (soft delete)
            table.DeleteRow(loaded);
            Assert.Equal(DataRowState.Deleted, loaded.DataRowState);

            // Act
            IDataSet? changeset = dataSet.CreateChangeset();

            // Assert
            Assert.NotNull(changeset);
            Assert.True(changeset!.Tables.ContainsKey("Department"));

            IDataTable csTable = changeset.Tables["Department"];
            Assert.Equal(1, csTable.Rows.Count);

            IDataRow csRow = csTable.Rows[0];
            Assert.Equal(DataRowState.Deleted, csRow.DataRowState);

            // Deleted rows must include PK value (needed for DELETE)
            Assert.True(csRow.ContainsKey("Id"));
            Assert.Equal(10, (int)csRow["Id"]!);

            // Optional client key is included when present in schema
            Assert.True(csRow.ContainsKey(SpecialColumnNames.CLIENT_KEY));

            // Unrelated columns exist in schema, but must not have values copied
            Assert.False(csRow.ContainsKey("Name"));
}

        [Fact]
        public void CreateChangeset_DeletedRow_IncludesAllCompositePrimaryKeyValues()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Link");
            table.AddColumn("CompanyId", DataTypeNames.INT32, false, true);
            table.AddColumn("DepartmentId", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            table.PrimaryKeys = new List<string> { "CompanyId", "DepartmentId" };

            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(table.Columns);
            loaded["CompanyId"] = 1;
            loaded["DepartmentId"] = 2;
            loaded["Name"] = "X";
            loaded[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();
            table.AddLoadedRow(loaded);

            table.DeleteRow(loaded);
            Assert.Equal(DataRowState.Deleted, loaded.DataRowState);

            // Act
            IDataSet? changeset = dataSet.CreateChangeset();

            // Assert
            Assert.NotNull(changeset);

            IDataTable csTable = changeset!.Tables["Link"];
            Assert.Equal(1, csTable.Rows.Count);

            IDataRow csRow = csTable.Rows[0];
            Assert.Equal(DataRowState.Deleted, csRow.DataRowState);

            Assert.True(csRow.ContainsKey("CompanyId"));
            Assert.True(csRow.ContainsKey("DepartmentId"));

            Assert.Equal(1, (int)csRow["CompanyId"]!);
            Assert.Equal(2, (int)csRow["DepartmentId"]!);

            // Unrelated columns exist in schema, but must not have values copied
            Assert.False(csRow.ContainsKey("Name"));
}
    }
}
