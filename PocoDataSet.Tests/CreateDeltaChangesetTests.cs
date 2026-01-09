using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class CreateDeltaChangesetTests
    {
        [Fact]
        public void CreateDeltaChangeset_ReturnsNull_WhenDataSetIsNull()
        {
            // Arrange
            IDataSet? dataSet = null;

            // Act
            IDataSet? changeset = dataSet.CreateDeltaChangeset();

            // Assert
            Assert.Null(changeset);
        }

        [Fact]
        public void CreateDeltaChangeset_ReturnsEmptyDataSet_WhenNoChanges()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(table.Columns);
            loaded["Id"] = 1;
            loaded["Name"] = "HR";
            table.AddLoadedRow(loaded);

            Assert.Equal(DataRowState.Unchanged, loaded.DataRowState);

            // Act
            IDataSet? changeset = dataSet.CreateDeltaChangeset();

            // Assert
            Assert.NotNull(changeset);
            Assert.Equal(0, changeset!.Tables.Count);
        }

        [Fact]
        public void CreateDeltaChangeset_IncludesOnlyChangedRows_AndOnlyTablesWithChanges()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // Table A: has changes
            IDataTable a = dataSet.AddNewTable("A");
            a.AddColumn("Id", DataTypeNames.INT32);
            a.AddColumn("Name", DataTypeNames.STRING);

            // Unchanged loaded row
            IDataRow a1 = DataRowExtensions.CreateRowFromColumns(a.Columns);
            a1["Id"] = 1;
            a1["Name"] = "Unchanged";
            a.AddLoadedRow(a1);

            // Added row
            IDataRow a2 = DataRowExtensions.CreateRowFromColumns(a.Columns);
            a2["Id"] = 2;
            a2["Name"] = "Added";
            a.AddRow(a2);

            // Modified row (loaded then edited)
            IDataRow a3 = DataRowExtensions.CreateRowFromColumns(a.Columns);
            a3["Id"] = 3;
            a3["Name"] = "Before";
            a.AddLoadedRow(a3);
            a3["Name"] = "After";

            // Table B: no changes -> must not appear in changeset
            IDataTable b = dataSet.AddNewTable("B");
            b.AddColumn("Id", DataTypeNames.INT32);

            IDataRow b1 = DataRowExtensions.CreateRowFromColumns(b.Columns);
            b1["Id"] = 10;
            b.AddLoadedRow(b1);

            // Act
            IDataSet? changeset = dataSet.CreateDeltaChangeset();

            // Assert
            Assert.NotNull(changeset);

            Assert.True(changeset!.Tables.ContainsKey("A"));
            Assert.False(changeset.Tables.ContainsKey("B"));

            IDataTable csA = changeset.Tables["A"];
            Assert.Equal(2, csA.Rows.Count);

            bool hasAdded = false;
            bool hasModified = false;

            for (int i = 0; i < csA.Rows.Count; i++)
            {
                IDataRow r = csA.Rows[i];
                if (r.DataRowState == DataRowState.Added)
                {
                    hasAdded = true;
                }
                else if (r.DataRowState == DataRowState.Modified)
                {
                    hasModified = true;
                }
                else
                {
                    Assert.True(false, "Delta changeset must not include Unchanged rows.");
                }
            }

            Assert.True(hasAdded);
            Assert.True(hasModified);
        }

        [Fact]
        public void CreateDeltaChangeset_ModifiedRow_ContainsPrimaryKey_ClientKey_AndOnlyChangedColumns()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.AddColumn("Description", DataTypeNames.STRING);
            table.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            table.PrimaryKeys = new List<string> { "Id" };

            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(table.Columns);
            loaded["Id"] = 10;
            loaded["Name"] = "Before";
            loaded["Description"] = "Unchanged";
            loaded[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();

            table.AddLoadedRow(loaded);
            Assert.Equal(DataRowState.Unchanged, loaded.DataRowState);

            // Modify only Name
            loaded["Name"] = "After";
            Assert.Equal(DataRowState.Modified, loaded.DataRowState);

            // Act
            IDataSet? changeset = dataSet.CreateDeltaChangeset();

            // Assert
            Assert.NotNull(changeset);
            Assert.True(changeset!.Tables.ContainsKey("Department"));

            IDataTable csTable = changeset.Tables["Department"];
            Assert.Equal(1, csTable.Rows.Count);

            IDataRow csRow = csTable.Rows[0];
            Assert.Equal(DataRowState.Modified, csRow.DataRowState);

            // Required columns are present as values
            Assert.True(csRow.ContainsKey("Id"));
            Assert.True(csRow.ContainsKey(SpecialColumnNames.CLIENT_KEY));
            Assert.True(csRow.ContainsKey("Name"));

            // Unchanged column is not copied (value remains null/default)
            object? description;
            bool hasDescription = csRow.TryGetValue("Description", out description);
            Assert.True(hasDescription);
            Assert.Null(description);

            // Values are correct
            Assert.Equal(10, (int)csRow["Id"]!);
            Assert.Equal("After", (string)csRow["Name"]!);
        }

        [Fact]
        public void CreateDeltaChangeset_ModifiedRow_IncludesAllCompositePrimaryKeyColumns()
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
            loaded["Name"] = "Before";
            loaded[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();
            table.AddLoadedRow(loaded);

            loaded["Name"] = "After";
            Assert.Equal(DataRowState.Modified, loaded.DataRowState);

            // Act
            IDataSet? changeset = dataSet.CreateDeltaChangeset();

            // Assert
            Assert.NotNull(changeset);
            IDataTable csTable = changeset!.Tables["Link"];
            Assert.Equal(1, csTable.Rows.Count);

            IDataRow csRow = csTable.Rows[0];

            Assert.True(csRow.ContainsKey("CompanyId"));
            Assert.True(csRow.ContainsKey("DepartmentId"));
            Assert.True(csRow.ContainsKey("Name"));
            Assert.True(csRow.ContainsKey(SpecialColumnNames.CLIENT_KEY));

            Assert.Equal(1, (int)csRow["CompanyId"]!);
            Assert.Equal(2, (int)csRow["DepartmentId"]!);
        }


        [Fact]
        public void CreateDeltaChangeset_ModifiedRow_DoesNotIncludeColumn_WhenValueEqualsOriginal()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.AddColumn("Description", DataTypeNames.STRING);
            table.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            table.PrimaryKeys = new List<string> { "Id" };

            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(table.Columns);
            loaded["Id"] = 1;
            loaded["Name"] = "Sales";
            loaded["Description"] = "Original";
            loaded[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();

            table.AddLoadedRow(loaded);

            // Modify then revert back to original
            loaded["Description"] = "Changed";
            loaded["Description"] = "Original";

            // Ensure row is Modified or Unchanged depending on your row-state rules.
            // If your implementation flips back to Unchanged automatically, this test
            // should assert that the row is not included. If it stays Modified, then
            // delta must not include "Description" as a changed value.
            DataRowState state = loaded.DataRowState;

            // Act
            IDataSet? changeset = dataSet.CreateDeltaChangeset();

            // Assert
            Assert.NotNull(changeset);

            if (state == DataRowState.Unchanged)
            {
                // If reverting restores Unchanged, the row must not be included at all.
                Assert.False(changeset!.Tables.ContainsKey("Department"));
                return;
            }

            Assert.True(changeset!.Tables.ContainsKey("Department"));
            IDataTable csTable = changeset.Tables["Department"];
            Assert.Equal(1, csTable.Rows.Count);

            IDataRow csRow = csTable.Rows[0];
            Assert.Equal(DataRowState.Modified, csRow.DataRowState);

            // PK and client key must be present
            Assert.Equal(1, (int)csRow["Id"]!);
            Assert.True(csRow.ContainsKey(SpecialColumnNames.CLIENT_KEY));

            // "Description" exists in schema, but should NOT have been copied (null = not included)
            object? descriptionValue = null;
            Assert.True(csRow.TryGetValue("Description", out descriptionValue));
            Assert.Null(descriptionValue);
        }

        [Fact]
        public void CreateDeltaChangeset_TwoModifiedRows_WithDifferentChangedColumns_AreSparseIndependently()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.AddColumn("Description", DataTypeNames.STRING);
            table.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            table.PrimaryKeys = new List<string> { "Id" };

            // Row 1
            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(table.Columns);
            r1["Id"] = 1;
            r1["Name"] = "Sales";
            r1["Description"] = "D1";
            r1[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();
            table.AddLoadedRow(r1);

            // Row 2
            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(table.Columns);
            r2["Id"] = 2;
            r2["Name"] = "HR";
            r2["Description"] = "D2";
            r2[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();
            table.AddLoadedRow(r2);

            // Modify different columns
            r1["Name"] = "Sales Updated";          // only Name changed
            r2["Description"] = "D2 Updated";      // only Description changed

            // Act
            IDataSet? changeset = dataSet.CreateDeltaChangeset();

            // Assert
            Assert.NotNull(changeset);
            Assert.True(changeset!.Tables.ContainsKey("Department"));

            IDataTable csTable = changeset.Tables["Department"];
            Assert.Equal(2, csTable.Rows.Count);

            // Find delta rows by Id
            IDataRow? cs1 = null;
            IDataRow? cs2 = null;

            for (int i = 0; i < csTable.Rows.Count; i++)
            {
                IDataRow row = csTable.Rows[i];
                int id = (int)row["Id"]!;
                if (id == 1)
                    cs1 = row;
                if (id == 2)
                    cs2 = row;
            }

            Assert.NotNull(cs1);
            Assert.NotNull(cs2);

            Assert.Equal(DataRowState.Modified, cs1!.DataRowState);
            Assert.Equal(DataRowState.Modified, cs2!.DataRowState);

            // Row1: Name copied, Description must be null (not copied)
            Assert.Equal("Sales Updated", (string)cs1["Name"]!);
            object? cs1Description = null;
            Assert.True(cs1.TryGetValue("Description", out cs1Description));
            Assert.Null(cs1Description);

            // Row2: Description copied, Name must be null (not copied)
            Assert.Equal("D2 Updated", (string)cs2["Description"]!);
            object? cs2Name = null;
            Assert.True(cs2.TryGetValue("Name", out cs2Name));
            Assert.Null(cs2Name);
        }

        [Fact]
        public void CreateDeltaChangeset_ModifiedRow_IncludesColumn_WhenOriginalValueMissing()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            table.PrimaryKeys = new List<string> { "Id" };

            // Create a row that has current values but no original baseline
            // (Added row manually switched to Modified)
            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 123;
            row["Name"] = "X";
            row[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();

            table.AddRow(row); // Added
            row.SetDataRowState(DataRowState.Modified); // force "Modified" without originals

            object? original = null;
            bool hasOriginal = row.TryGetOriginalValue("Name", out original);
            Assert.False(hasOriginal);

            // Act
            IDataSet? changeset = dataSet.CreateDeltaChangeset();

            // Assert
            Assert.NotNull(changeset);
            Assert.True(changeset!.Tables.ContainsKey("Department"));

            IDataTable csTable = changeset.Tables["Department"];
            Assert.Equal(1, csTable.Rows.Count);

            IDataRow csRow = csTable.Rows[0];
            Assert.Equal(DataRowState.Modified, csRow.DataRowState);

            // PK copied
            Assert.Equal(123, (int)csRow["Id"]!);

            // Name should be copied (treated as changed because original missing)
            Assert.Equal("X", (string)csRow["Name"]!);
        }
/*
        [Fact]
        public void CreateDeltaChangeset_ModifiedRow_SkipsColumn_WhenTryGetValueReturnsFalse()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.AddColumn("Description", DataTypeNames.STRING);

            table.PrimaryKeys = new List<string> { "Id" };

            // IMPORTANT: create a row that does NOT contain "Description"
            // (simulate schema-evolution / sparse rows)
            List<IColumnMetadata> subsetColumns = new List<IColumnMetadata>();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName != "Description")
                {
                    subsetColumns.Add(table.Columns[i]);
                }
            }

            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(subsetColumns);
            loaded["Id"] = 1;
            loaded["Name"] = "Sales";

            table.AddLoadedRow(loaded);

            // Ensure the row really lacks "Description"
            object? desc = null;
            Assert.False(loaded.TryGetValue("Description", out desc));
            Assert.False(loaded.ContainsKey("Description"));

            // Modify a real column so row is Modified
            loaded["Name"] = "Sales Updated";
            Assert.Equal(DataRowState.Modified, loaded.DataRowState);

            // Act
            IDataSet? changeset = dataSet.CreateDeltaChangeset();

            // Assert
            Assert.NotNull(changeset);
            Assert.True(changeset!.Tables.ContainsKey("Department"));

            IDataTable csTable = changeset.Tables["Department"];
            Assert.Equal(1, csTable.Rows.Count);

            IDataRow csRow = csTable.Rows[0];
            Assert.Equal(DataRowState.Modified, csRow.DataRowState);

            // Changed column copied
            Assert.Equal("Sales Updated", (string)csRow["Name"]!);

            // "Description" is in schema (table schema copied),
            // but must not have been copied as a value.
            object? csDesc = null;
            Assert.True(csRow.TryGetValue("Description", out csDesc));
            Assert.Null(csDesc);
        }
*/
        [Fact]
        public void CreateDeltaChangeset_ModifiedRow_DoesNotRequireClientKey_WhenTableDoesNotContainClientKey()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.AddColumn("Description", DataTypeNames.STRING);

            table.PrimaryKeys = new List<string> { "Id" };

            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(table.Columns);
            loaded["Id"] = 5;
            loaded["Name"] = "HR";
            loaded["Description"] = "D";
            table.AddLoadedRow(loaded);

            // Modify one column
            loaded["Name"] = "HR Updated";
            Assert.Equal(DataRowState.Modified, loaded.DataRowState);

            // Act
            IDataSet? changeset = dataSet.CreateDeltaChangeset();

            // Assert
            Assert.NotNull(changeset);
            Assert.True(changeset!.Tables.ContainsKey("Department"));

            IDataTable csTable = changeset.Tables["Department"];
            Assert.Equal(1, csTable.Rows.Count);

            IDataRow csRow = csTable.Rows[0];
            Assert.Equal(DataRowState.Modified, csRow.DataRowState);

            // PK copied
            Assert.Equal(5, (int)csRow["Id"]!);

            // Changed column copied
            Assert.Equal("HR Updated", (string)csRow["Name"]!);

            // Client key must NOT be present because the table schema does not contain it
            Assert.False(csRow.ContainsKey(SpecialColumnNames.CLIENT_KEY));
        }

    }
}
