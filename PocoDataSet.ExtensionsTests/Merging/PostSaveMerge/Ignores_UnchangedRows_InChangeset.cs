using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the contract that PostSave merge processes only Added/Modified/Deleted rows
    /// from the changeset snapshot. Unchanged rows in the changeset must be ignored.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void Ignores_UnchangedRows_InChangeset()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow baseline = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            baseline["Id"] = 10;
            baseline["Name"] = "Before";
            currentTable.AddLoadedRow(baseline);

            // Create a local pending change
            baseline["Name"] = "Edited Locally";
            Assert.Equal(DataRowState.Modified, baseline.DataRowState);
            Assert.True(baseline.HasOriginalValues);

            // Changeset row with the same PK, but kept Unchanged on purpose.
            // PostSave must ignore it (no AcceptChanges, no overwrite).
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable changesetTable = changeset.AddNewTable("Department");
            changesetTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            changesetTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow serverRowUnchanged = DataRowExtensions.CreateRowFromColumns(changesetTable.Columns);
            serverRowUnchanged["Id"] = 10;
            serverRowUnchanged["Name"] = "Server Value That Must Be Ignored";
            changesetTable.AddLoadedRow(serverRowUnchanged);
            Assert.Equal(DataRowState.Unchanged, serverRowUnchanged.DataRowState);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoPostSaveMerge(changeset, options);

            // Assert
            Assert.Single(currentTable.Rows);
            Assert.Equal("Edited Locally", (string)baseline["Name"]!);
            Assert.Equal(DataRowState.Modified, baseline.DataRowState);
            Assert.True(baseline.HasOriginalValues);
        }
    }
}
