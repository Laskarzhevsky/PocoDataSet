using PocoDataSet.Extensions;
using PocoDataSet.IData;
using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the invariant that a single MergeOptions instance (and its DataSetMergeResult
    /// instance) flows through PostSave merge without being replaced by inner layers. IMPORTANT:
    /// PostSave merge only processes rows from the CHANGETSET/REFRESHED table that are
    /// Added/Modified/Deleted. Unchanged rows are ignored.
    ///
    /// Scenario:
    /// - Build a CURRENT DataSet (the client-side truth before the merge).
    /// - Build a REFRESHED/CHANGESET DataSet (the server-side snapshot or post-save response).
    /// - Execute PostSave merge (apply server post-save changeset onto current).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
    /// Notes:
    /// - This file contains exactly one test method: Keeps_MergeResult_Instance.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void Keeps_MergeResult_Instance()
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
