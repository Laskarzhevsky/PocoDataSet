using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// A stress test that performs randomized operations and merges to ensure key *RefreshPreservingLocalChanges*
        /// invariants always hold.  Invariants typically validated here include: - No duplicate keys after merge. - Row
        /// states remain valid (no impossible transitions). - Local pending changes (Added/Modified/Deleted) are never
        /// silently lost. - Merge does not throw for allowed inputs.  The goal is to catch edge-case regressions that
        /// targeted unit tests might miss.
        /// </summary>

        [Fact]
        public void Torture_RandomOps_MaintainInvariants()
        {
            // Deterministic seed to make failures reproducible
            Random random = new Random(12345);

            // Build initial dataset with one table and PK
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            // Add initial loaded rows (server-origin)
            int nextId = 1;
            for (int i = 0; i < 25; i++)
            {
                IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
                row["Id"] = nextId;
                row["Name"] = "Dept_" + nextId;
                table.AddLoadedRow(row);
                nextId++;
            }

            // Torture loop: many steps, periodic checks
            for (int step = 1; step <= 400; step++)
            {
                int action = random.Next(0, 6);

                if (action == 0)
                {
                    // Add a new client-side row (Added)
                    IDataRow r = table.AddNewRow();
                    r["Id"] = nextId;
                    r["Name"] = "New_" + nextId;
                    nextId++;
                }
                else if (action == 1)
                {
                    // Modify a random non-deleted row
                    IDataRow? r = PickRandomRow(table, random, allowDeleted: false);
                    if (r != null)
                    {
                        // Avoid PK edits; modify Name only
                        object? idObj;
                        r.TryGetValue("Id", out idObj);

                        int id = 0;
                        if (idObj != null)
                        {
                            id = (int)idObj;
                        }

                        r["Name"] = "Edited_" + id + "_" + step;
                    }
                }
                else if (action == 2)
                {
                    // Delete a random non-deleted row
                    IDataRow? r = PickRandomRow(table, random, allowDeleted: false);
                    if (r != null)
                    {
                        table.DeleteRow(r);
                    }
                }
                else if (action == 3)
                {
                    // CreateChangeset invariants (fast and frequent)
                    IDataSet? changeset = dataSet.CreateChangeset();
                    ValidateChangesetInvariants(dataSet, changeset);
                }
                else if (action == 4)
                {
                    // AcceptChanges invariants
                    int expectedRemaining = CountNonDeletedRows(table);
                    table.AcceptChanges();

                    // After AcceptChanges:
                    // - no Deleted rows exist
                    // - all remaining are Unchanged
                    Assert.Equal(expectedRemaining, table.Rows.Count);

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        IDataRow row = table.Rows[i];
                        Assert.NotEqual(DataRowState.Deleted, row.DataRowState);
                        Assert.Equal(DataRowState.Unchanged, row.DataRowState);
                    }
                }
                else
                {
                    // Refresh merge invariants
                    IDataSet refreshed = BuildRefreshedSnapshotFromCurrent(table, random);

                    // Must not throw
                    MergeOptions options = new MergeOptions();
            // Execute RefreshPreservingLocalChanges merge: refresh server values where safe, while preserving local Added/Modified/Deleted rows.
            dataSet.DoRefreshMergePreservingLocalChanges(refreshed, options);
// Invariants after merge:
                    // - no duplicate PK among non-deleted rows
                    // - no null PK values among rows that have the column (int PK should always exist)
                    AssertNoDuplicatePrimaryKeys(table, "Id");
                }

                // Always validate core invariants periodically
                if (step % 25 == 0)
                {
                    AssertNoDuplicatePrimaryKeys(table, "Id");
                }
            }
        }
    }
}
