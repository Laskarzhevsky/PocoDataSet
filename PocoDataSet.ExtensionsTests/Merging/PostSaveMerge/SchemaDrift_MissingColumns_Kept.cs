using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the PostSave **schema contract** when the server changeset is missing columns that exist
    /// in the CURRENT schema.
    ///
    /// PostSave is a value merge into CURRENT; it must not "downgrade" schema based on the server
    /// response shape.
    ///
    /// Scenario:
    /// - CURRENT table schema: (Id PK, Name, Age)
    /// - CURRENT has a loaded row with Age populated.
    /// - SERVER PostSave changeset schema: (Id PK, Name)   // Age column is missing
    /// - SERVER changeset row is Modified and changes Name.
    ///
    /// Expected behavior:
    /// - PostSave applies server values for columns present in the changeset (Id, Name).
    /// - PostSave does NOT remove the Age column from CURRENT schema.
    /// - Because Age is not present in the changeset row, PostSave does not overwrite it; the value stays intact.
    ///
    /// How this test proves it:
    /// - Arrange builds CURRENT with an extra column (Age) and a non-default Age value.
    /// - Arrange builds a SERVER changeset table without Age and with a Modified row updating Name.
    /// - Act runs PostSave merge.
    /// - Assert checks:
    ///   (1) CURRENT still has the Age column,
    ///   (2) Age value is preserved,
    ///   (3) Name is updated and row is accepted (Unchanged).
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void SchemaDrift_MissingColumns_Kept()
        {
            // Arrange: CURRENT schema contains a column the server does not return.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Person");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn("Age", DataTypeNames.INT32);

            IDataRow currentRow = new DataRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Before";
            currentRow["Age"] = 42;
            t.AddRow(currentRow);

            // Mark as "loaded from DB" before PostSave runs.
            currentRow.AcceptChanges();
            Assert.Equal(DataRowState.Unchanged, currentRow.DataRowState);

            // Arrange: SERVER changeset does NOT include the "Age" column.
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable ct = changeset.AddNewTable("Person");
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            IDataRow serverRow = new DataRow();
            serverRow["Id"] = 1;
            serverRow["Name"] = "Before";
            ct.AddRow(serverRow);

            // Server row must be Modified so PostSave processes it.
            serverRow.AcceptChanges();
            serverRow["Name"] = "After (server)";
            Assert.Equal(DataRowState.Modified, serverRow.DataRowState);

            MergeOptions options = new MergeOptions();

            // Act: apply server PostSave changeset onto current.
            t.DoPostSaveMerge(ct, options);

            // Assert: schema is preserved (Age still exists).
            Assert.Equal(3, t.Columns.Count);

            bool hasAge = false;
            for (int i = 0; i < t.Columns.Count; i++)
            {
                if (t.Columns[i].ColumnName == "Age")
                {
                    hasAge = true;
                    break;
                }
            }

            Assert.True(hasAge);

            // Assert: missing column in changeset does not overwrite existing value.
            Assert.Equal(42, currentRow["Age"]);

            // Assert: server-provided column value is applied.
            Assert.Equal("After (server)", currentRow["Name"]);

            // PostSave accepts the row (it represents persisted truth now).
            Assert.Equal(DataRowState.Unchanged, currentRow.DataRowState);
        }
    }
}
