using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Merge result invariants: Added/Updated entries must refer to rows present in the final
    /// table, Deleted entries must refer to PKs not present in the final table. These tests protect
    /// against future refactors that drift merge result reporting away from actual table state.
    ///
    /// Scenario:
    /// - Build a CURRENT DataSet (the client-side truth before the merge).
    /// - Build a REFRESHED/CHANGESET DataSet (the server-side snapshot or post-save response).
    /// - Execute RefreshIfNoChangesExist merge (throws if current has pending changes).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
    /// Notes:
    /// - This file contains exactly one test method: ResultEntries_Match_FinalState.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void ResultEntries_Match_FinalState()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            IDataRow c2 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c2["Id"] = 2;
            c2["Name"] = "Two";
            t.AddLoadedRow(c2);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r2["Id"] = 2;
            r2["Name"] = "Two (updated)";
            rt.AddLoadedRow(r2);

            IDataRow r3 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r3["Id"] = 3;
            r3["Name"] = "Three";
            rt.AddLoadedRow(r3);

            MergeOptions options = new MergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert - counts
            Assert.Single(options.DataSetMergeResult.AddedDataRows);
            Assert.Single(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Single(options.DataSetMergeResult.DeletedDataRows);

            int addedId = (int)options.DataSetMergeResult.AddedDataRows[0].DataRow["Id"]!;
            int updatedId = (int)options.DataSetMergeResult.UpdatedDataRows[0].DataRow["Id"]!;
            int deletedId = (int)options.DataSetMergeResult.DeletedDataRows[0].DataRow["Id"]!;

            Assert.True(RowExistsById(t, addedId));
            Assert.True(RowExistsById(t, updatedId));
            Assert.False(RowExistsById(t, deletedId));

            Assert.Equal(2, t.Rows.Count);
            Assert.True(RowExistsById(t, 2));
            Assert.True(RowExistsById(t, 3));
        }

        private static bool RowExistsById(IDataTable t, int id)
        {
            for (int i = 0; i < t.Rows.Count; i++)
            {
                if ((int)t.Rows[i]["Id"]! == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
