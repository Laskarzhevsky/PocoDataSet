using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Additional "no-op refresh" stability coverage. Locks that RefreshPreservingLocalChanges does
    /// not record any changes in DataSetMergeResult and does not churn row instances when refreshed
    /// values are identical.
    ///
    /// Scenario:
    /// - Build a CURRENT DataSet (the client-side truth before the merge).
    /// - Build a REFRESHED/CHANGESET DataSet (the server-side snapshot or post-save response).
    /// - Execute RefreshPreservingLocalChanges merge (refresh unchanged rows, preserve local changes).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
    /// Notes:
    /// - This file contains exactly one test method: IdenticalValues_NoMergeResultEntries_NoChurn.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void IdenticalValues_NoMergeResultEntries_NoChurn()
        {
            // ------------------------------------------------------------
            // Arrange
            // ------------------------------------------------------------
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            c1["Id"] = 1;
            c1["Name"] = "Sales";
            currentTable.AddLoadedRow(c1);

            IDataRow c2 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            c2["Id"] = 2;
            c2["Name"] = "HR";
            currentTable.AddLoadedRow(c2);

            IDataRow c3 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            c3["Id"] = 3;
            c3["Name"] = "IT";
            currentTable.AddLoadedRow(c3);

            Assert.Equal(DataRowState.Unchanged, c1.DataRowState);
            Assert.Equal(DataRowState.Unchanged, c2.DataRowState);
            Assert.Equal(DataRowState.Unchanged, c3.DataRowState);
            Assert.False(c1.HasOriginalValues);
            Assert.False(c2.HasOriginalValues);
            Assert.False(c3.HasOriginalValues);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            r1["Id"] = 1;
            r1["Name"] = "Sales";
            refreshedTable.AddLoadedRow(r1);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            r2["Id"] = 2;
            r2["Name"] = "HR";
            refreshedTable.AddLoadedRow(r2);

            IDataRow r3 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            r3["Id"] = 3;
            r3["Name"] = "IT";
            refreshedTable.AddLoadedRow(r3);

            MergeOptions options = new MergeOptions();

            // Sanity: starting result must be empty.
            Assert.Empty(options.DataSetMergeResult.AddedDataRows);
            Assert.Empty(options.DataSetMergeResult.DeletedDataRows);
            Assert.Empty(options.DataSetMergeResult.UpdatedDataRows);

            // ------------------------------------------------------------
            // Act
            // ------------------------------------------------------------
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // ------------------------------------------------------------
            // Assert
            // ------------------------------------------------------------
            Assert.Equal(3, currentTable.Rows.Count);
            Assert.Same(c1, currentTable.Rows[0]);
            Assert.Same(c2, currentTable.Rows[1]);
            Assert.Same(c3, currentTable.Rows[2]);

            Assert.Equal(DataRowState.Unchanged, c1.DataRowState);
            Assert.Equal(DataRowState.Unchanged, c2.DataRowState);
            Assert.Equal(DataRowState.Unchanged, c3.DataRowState);
            Assert.False(c1.HasOriginalValues);
            Assert.False(c2.HasOriginalValues);
            Assert.False(c3.HasOriginalValues);

            Assert.Empty(options.DataSetMergeResult.AddedDataRows);
            Assert.Empty(options.DataSetMergeResult.DeletedDataRows);
            Assert.Empty(options.DataSetMergeResult.UpdatedDataRows);
        }
    }
}
