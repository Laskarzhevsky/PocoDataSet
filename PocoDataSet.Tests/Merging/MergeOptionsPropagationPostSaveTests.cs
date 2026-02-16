using PocoDataSet.Extensions;
using PocoDataSet.IData;
using Xunit;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// Locks the invariant that a single MergeOptions instance (and its DataSetMergeResult instance)
    /// flows through PostSave merge without being replaced by inner layers.
    ///
    /// IMPORTANT: PostSave merge only processes rows from the CHANGETSET/REFRESHED table that are
    /// Added/Modified/Deleted. Unchanged rows are ignored.
    /// </summary>
    public class MergeOptionsPropagationPostSaveTests
    {
        [Fact]
        public void PostSaveMerge_DoesNotReplace_DataSetMergeResult_Instance()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(t.Columns);
            currentRow["Id"] = 1;
            currentRow["Name"] = "Temp";
            t.AddLoadedRow(currentRow); // baseline row, Unchanged

            // PostSave consumes a CHANGSET table: rows must be Added/Modified/Deleted to be applied.
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable ct = changeset.AddNewTable("T");
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            IDataRow changesetRow = DataRowExtensions.CreateRowFromColumns(ct.Columns);
            changesetRow["Id"] = 1;
            changesetRow["Name"] = "Saved";
            ct.AddRow(changesetRow); // Added state (required for PostSave to apply)

            MergeOptions options = new MergeOptions();
            IDataSetMergeResult expectedResultInstance = options.DataSetMergeResult;

            // Act
            current.DoPostSaveMerge(changeset, options);

            // Assert: options/result instance must not be replaced.
            Assert.Same(expectedResultInstance, options.DataSetMergeResult);

            // Sanity: post-save values applied.
            Assert.Equal("Saved", (string)current.Tables["T"].Rows[0]["Name"]!);
        }
    }
}
