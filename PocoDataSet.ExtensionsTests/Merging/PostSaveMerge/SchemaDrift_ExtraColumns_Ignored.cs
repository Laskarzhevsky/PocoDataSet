using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the PostSave **schema contract**: PostSave merges values into the CURRENT schema and must not
    /// add or remove columns based on the server changeset shape.
    ///
    /// Scenario:
    /// - CURRENT table has columns: (Id PK, Name).
    /// - SERVER changeset table has columns: (Id PK, Name, ExtraServerOnlyColumn).
    ///
    /// Expected behavior:
    /// - PostSave updates row values for columns that exist in CURRENT and are present in the changeset row.
    /// - The extra column from the server is ignored (CURRENT schema remains unchanged).
    ///
    /// How this test proves it:
    /// - Arrange creates current with two columns + one loaded row.
    /// - Arrange creates changeset with an extra column and a Modified row with new Name.
    /// - Act merges.
    /// - Assert verifies:
    ///   (1) Name was updated,
    ///   (2) CURRENT still has exactly two columns (Extra was not added).
    ///
    /// Notes:
    /// - This file contains exactly one test method: SchemaDrift_ExtraColumns_Ignored.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void SchemaDrift_ExtraColumns_Ignored()
        {
            // Arrange (CURRENT)
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(t.Columns);
            currentRow["Id"] = 10;
            currentRow["Name"] = "Before";
            t.AddLoadedRow(currentRow);

            Assert.Equal(2, t.Columns.Count);

            // Arrange (CHANGESET) with extra server-only column.
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable ct = changeset.AddNewTable("Department");
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);
            ct.AddColumn("Extra", DataTypeNames.STRING); // server-only

            IDataRow serverRow = DataRowExtensions.CreateRowFromColumns(ct.Columns);
            serverRow["Id"] = 10;
            serverRow["Name"] = "After";
            serverRow["Extra"] = "Server only";

            ct.AddLoadedRow(serverRow);
            serverRow.AcceptChanges();

            // Make the changeset row truly Modified by changing Name to a DIFFERENT value after AcceptChanges.
            // (Setting the same value would keep the row Unchanged and would not exercise the "apply server updates" path.)
            serverRow["Name"] = "After (server)";
            Assert.Equal(DataRowState.Modified, serverRow.DataRowState);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoPostSaveMerge(changeset, options);

            // Assert
            Assert.Equal("After (server)", (string)t.Rows[0]["Name"]!);

            // CURRENT schema remains unchanged: extra column not added.
            Assert.Equal(2, t.Columns.Count);
        }
    }
}
