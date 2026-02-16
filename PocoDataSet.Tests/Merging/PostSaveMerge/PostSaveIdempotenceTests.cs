using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// PostSave hardening: idempotence.
    /// Running PostSave twice with the same changeset must produce no changes on the second run.
    /// </summary>
    public sealed class PostSaveIdempotenceTests
    {
        [Fact]
        public void PostSave_WhenRunTwice_WithSameChangeset_IsNoOpSecondTime()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(t.Columns);
            currentRow["Id"] = 1;
            currentRow["Name"] = "Dirty";
            t.AddLoadedRow(currentRow);

            // Simulate pending local change (Modified row).
            currentRow["Name"] = "Dirty (Modified)";
            Assert.Equal(DataRowState.Modified, currentRow.DataRowState);

            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable ct = changeset.AddNewTable("T");
            ct.AddColumn("Id", DataTypeNames.INT32);
            ct.AddColumn("Name", DataTypeNames.STRING);

            // PostSave processes only Added/Modified/Deleted rows from the changeset.
            // Using AddRow makes the row "Added" in the changeset, which is processed.
            IDataRow serverRow = DataRowExtensions.CreateRowFromColumns(ct.Columns);
            serverRow["Id"] = 1;
            serverRow["Name"] = "Saved";
            ct.AddRow(serverRow);

            // Act: first PostSave applies saved values and accepts changes in current.
            MergeOptions firstOptions = new MergeOptions();
            current.DoPostSaveMerge(changeset, firstOptions);

            // Assert: first run applied values and accepted changes.
            Assert.Equal("Saved", (string)current.Tables["T"].Rows[0]["Name"]!);
            Assert.Equal(DataRowState.Unchanged, current.Tables["T"].Rows[0].DataRowState);

            // Act: run PostSave again with the same changeset.
            MergeOptions secondOptions = new MergeOptions();
            current.DoPostSaveMerge(changeset, secondOptions);

            // Assert: second run must be a no-op (no merge result entries).
            Assert.Empty(secondOptions.DataSetMergeResult.AddedDataRows);
            Assert.Empty(secondOptions.DataSetMergeResult.DeletedDataRows);
            Assert.Empty(secondOptions.DataSetMergeResult.UpdatedDataRows);

            // And state remains stable.
            Assert.Single(current.Tables["T"].Rows);
            Assert.Equal("Saved", (string)current.Tables["T"].Rows[0]["Name"]!);
            Assert.Equal(DataRowState.Unchanged, current.Tables["T"].Rows[0].DataRowState);
        }
    }
}