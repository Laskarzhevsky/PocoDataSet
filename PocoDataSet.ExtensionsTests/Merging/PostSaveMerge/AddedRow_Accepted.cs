using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the PostSave contract that a CURRENT <see cref="DataRowState.Added"/> row becomes
    /// <see cref="DataRowState.Unchanged"/> after a successful PostSave merge.
    ///
    /// Scenario:
    /// - CURRENT contains an Added row (client created it and is about to save).
    /// - SERVER PostSave changeset returns the same row (by PK) marked Added (server confirms insert).
    ///
    /// Expected behavior:
    /// - PostSave copies any server-confirmed values and calls AcceptChanges.
    /// - The row ends Unchanged (it now represents the persisted server snapshot).
    /// - The row instance remains the same.
    ///
    /// How this test proves it:
    /// - Arrange creates a current Added row with a real PK (no server-assigned identity needed here).
    /// - Arrange creates a changeset Added row with matching PK and the same values.
    /// - Act merges.
    /// - Assert checks instance stability and Added -> Unchanged state transition.
    ///
    /// Notes:
    /// - This file contains exactly one test method: AddedRow_Accepted.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void AddedRow_Accepted()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(t.Columns);
            currentRow["Id"] = 10;
            currentRow["Name"] = "Customer Service";

            // Current row is Added because it is new locally.
            t.AddRow(currentRow);
            Assert.Equal(DataRowState.Added, currentRow.DataRowState);

            // Server changeset echoes the insert back (Added) for the same PK.
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable ct = changeset.AddNewTable("Department");
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            IDataRow serverRow = DataRowExtensions.CreateRowFromColumns(ct.Columns);
            serverRow["Id"] = 10;
            serverRow["Name"] = "Customer Service";

            ct.AddRow(serverRow);
            serverRow.SetDataRowState(DataRowState.Added);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoPostSaveMerge(changeset, options);

            // Assert
            // Same instance is preserved.
            Assert.Same(currentRow, t.Rows[0]);

            // Added -> Unchanged after AcceptChanges as part of PostSave.
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);
        }
    }
}
