using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class CreateChangesetDeletedRowsTests
    {
        [Fact]
        public void CreateChangeset_IncludesDeletedRow_AndContainsPrimaryKeyColumn()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(table.Columns);
            loaded["Id"] = 10;
            loaded["Name"] = "HR";
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

            // Column subset correctness (minimum contract): PK column must exist in changeset schema.
            bool hasIdColumn = false;
            for (int i = 0; i < csTable.Columns.Count; i++)
            {
                if (csTable.Columns[i].ColumnName == "Id")
                {
                    hasIdColumn = true;
                    break;
                }
            }

            Assert.True(hasIdColumn);

            // And the PK value must be present (needed for DELETE).
            Assert.Equal(10, (int)csRow["Id"]!);
        }

        [Fact]
        public void CreateChangeset_ForInsertAndUpdate_ContainsRequiredColumns()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.AddColumn("RowVersion", DataTypeNames.BINARY);

            // Added row (insert scenario)
            IDataRow added = DataRowExtensions.CreateRowFromColumns(table.Columns);
            added["Id"] = 0;
            added["Name"] = "New";
            added[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();
            added["RowVersion"] = null;
            table.AddRow(added);

            // Loaded then modified (update scenario)
            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(table.Columns);
            loaded["Id"] = 5;
            loaded["Name"] = "Before";
            loaded[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();
            loaded["RowVersion"] = new byte[] { 1, 2, 3 };
            table.AddLoadedRow(loaded);

            loaded["Name"] = "After";
            Assert.Equal(DataRowState.Modified, loaded.DataRowState);

            // Act
            IDataSet? changeset = dataSet.CreateChangeset();

            // Assert
            Assert.NotNull(changeset);
            Assert.True(changeset!.Tables.ContainsKey("Department"));

            IDataTable csTable = changeset.Tables["Department"];

            // Minimum contract: these columns must exist in changeset schema
            Assert.True(HasColumn(csTable, "Id"));
            Assert.True(HasColumn(csTable, "Name"));
            Assert.True(HasColumn(csTable, SpecialColumnNames.CLIENT_KEY));
            Assert.True(HasColumn(csTable, "RowVersion"));

            // And we should have 2 rows (Added + Modified)
            Assert.Equal(2, csTable.Rows.Count);
        }

        private static bool HasColumn(IDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == columnName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
