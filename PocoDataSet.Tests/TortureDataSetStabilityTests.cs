using System;
using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class TortureDataSetStabilityTests
    {
        [Fact]
        public void Torture_RandomOperations_DoNotCorruptState_AndMaintainInvariants()
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
                    dataSet.MergeWith(refreshed, MergeMode.Refresh);

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

        private static IDataRow? PickRandomRow(IDataTable table, Random random, bool allowDeleted)
        {
            if (table.Rows.Count == 0)
            {
                return null;
            }

            // Try a few times to find a row matching criteria
            for (int attempt = 0; attempt < 10; attempt++)
            {
                int index = random.Next(0, table.Rows.Count);
                IDataRow row = table.Rows[index];

                if (!allowDeleted && row.DataRowState == DataRowState.Deleted)
                {
                    continue;
                }

                return row;
            }

            // Fallback scan
            for (int i = 0; i < table.Rows.Count; i++)
            {
                IDataRow row = table.Rows[i];
                if (allowDeleted)
                {
                    return row;
                }

                if (row.DataRowState != DataRowState.Deleted)
                {
                    return row;
                }
            }

            return null;
        }

        private static int CountNonDeletedRows(IDataTable table)
        {
            int count = 0;

            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i].DataRowState != DataRowState.Deleted)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AssertNoDuplicatePrimaryKeys(IDataTable table, string pkColumnName)
        {
            HashSet<int> keys = new HashSet<int>();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                IDataRow row = table.Rows[i];

                // Ignore deleted rows for uniqueness among active rows
                if (row.DataRowState == DataRowState.Deleted)
                {
                    continue;
                }

                object? value;
                row.TryGetValue(pkColumnName, out value);

                // For int PK, value must exist (your AddRow/AddLoadedRow ensure columns exist)
                Assert.NotNull(value);

                int id = (int)value!;

                bool added = keys.Add(id);
                Assert.True(added);
            }
        }

        private static void ValidateChangesetInvariants(IDataSet source, IDataSet? changeset)
        {
            if (changeset == null)
            {
                // Your CreateChangeset returns non-null for non-null input, but be defensive.
                return;
            }

            // Invariant: changeset must contain only tables that have changes,
            // and within those tables only rows in Added/Modified/Deleted.
            foreach (KeyValuePair<string, IDataTable> kvp in changeset.Tables)
            {
                string tableName = kvp.Key;
                IDataTable csTable = kvp.Value;

                Assert.True(source.Tables.ContainsKey(tableName));

                for (int i = 0; i < csTable.Rows.Count; i++)
                {
                    IDataRow row = csTable.Rows[i];

                    Assert.NotEqual(DataRowState.Unchanged, row.DataRowState);
                    Assert.NotEqual(DataRowState.Detached, row.DataRowState);

                    bool ok =
                        row.DataRowState == DataRowState.Added ||
                        row.DataRowState == DataRowState.Modified ||
                        row.DataRowState == DataRowState.Deleted;

                    Assert.True(ok);
                }
            }
        }

        private static IDataSet BuildRefreshedSnapshotFromCurrent(IDataTable currentTable, Random random)
        {
            // Create a "server snapshot" dataset with same schema and same PK definition,
            // but randomly:
            // - omit some rows (simulating deletions on server)
            // - change some names (simulating updates on server)
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable t = refreshed.AddNewTable(currentTable.TableName);

            // Schema
            for (int c = 0; c < currentTable.Columns.Count; c++)
            {
                IColumnMetadata col = currentTable.Columns[c];
                t.AddColumn(col.ColumnName, col.DataType, col.IsNullable, col.IsPrimaryKey, col.IsForeignKey);
            }

            for (int i = 0; i < currentTable.Rows.Count; i++)
            {
                IDataRow src = currentTable.Rows[i];

                // Server snapshot only includes "persisted" rows (ignore Added client-only rows)
                if (src.DataRowState == DataRowState.Added)
                {
                    continue;
                }

                // Deleted client-side rows: server may still have them or not.
                // Randomly decide to include or omit.
                if (src.DataRowState == DataRowState.Deleted)
                {
                    int includeDeleted = random.Next(0, 2);
                    if (includeDeleted == 0)
                    {
                        continue;
                    }
                }

                int keep = random.Next(0, 100);
                if (keep < 15)
                {
                    // Omit row ~15% of the time
                    continue;
                }

                IDataRow dst = DataRowExtensions.CreateRowFromColumns(t.Columns);

                // Copy values
                for (int c = 0; c < t.Columns.Count; c++)
                {
                    string colName = t.Columns[c].ColumnName;
                    object? v;
                    src.TryGetValue(colName, out v);
                    dst[colName] = v;
                }

                // Random server update of Name
                int change = random.Next(0, 100);
                if (change < 30)
                {
                    object? idObj;
                    dst.TryGetValue("Id", out idObj);

                    int id = 0;
                    if (idObj != null)
                    {
                        id = (int)idObj;
                    }

                    dst["Name"] = "Server_" + id + "_" + random.Next(1000, 9999);
                }

                // Add as loaded
                t.AddLoadedRow(dst);
            }

            return refreshed;
        }
    }
}
