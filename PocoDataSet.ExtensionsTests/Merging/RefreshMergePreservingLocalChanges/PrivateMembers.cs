using PocoDataSet.Extensions;
using PocoDataSet.IData;

using System;
using System.Collections.Generic;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Composite primary key matrix (POCO).
    /// NOTE: Your current merge contract differs by mode:
    /// - RefreshPreservingLocalChanges rejects refreshed composite PK rows containing null parts (throws).
    /// - RefreshIfNoChangesExist currently allows refreshed composite PK rows containing null parts and treats them as non-correlatable.
    ///
    /// These tests lock the CURRENT observed behavior to prevent future regressions.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        private static IDataTable AddCompositePkTable(IDataSet ds)
        {
            IDataTable t = ds.AddNewTable("T");

            // Mark both columns as PK parts.
            // Kept nullable=true so we can construct null-part refreshed rows for contract tests.
            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            return t;
        }

        private static void AddLoadedRow(IDataTable t, object a, object? b, string name)
        {
            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["A"] = a;
            row["B"] = b;
            row["Name"] = name;
            t.AddLoadedRow(row);
        }

        private static bool ContainsCompositePk(IDataTable t, int a, string b)
        {
            for (int i = 0; i < t.Rows.Count; i++)
            {
                IDataRow r = t.Rows[i];
                if ((int)r["A"]! == a && (string)r["B"]! == b)
                {
                    return true;
                }
            }

            return false;
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

        private static IDataSet CreateCompositePkDataSetWithCurrentRow(object a, object? b)
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable t = dataSet.AddNewTable("T");

            // Make PK parts nullable so we can construct the invalid cases.
            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["A"] = a;
            row["B"] = b;
            row["Name"] = "Current";
            t.AddLoadedRow(row);

            return dataSet;
        }

        private static IDataSet CreateCompositePkRefreshedSnapshot(int a, string b, string name)
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable t = dataSet.AddNewTable("T");

            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["A"] = a;
            row["B"] = b;
            row["Name"] = name;
            t.AddLoadedRow(row);

            return dataSet;
        }

        private static IDataSet CreateCurrentCompositePk(int a, string b, string name)
        {
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(ds);

            AddLoadedRow(t, a, b, name);

            return ds;
        }

        private static IDataSet CreateRefreshedCompositePk(object objectA, object? objectB, string name)
        {
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(ds);

            AddLoadedRow(t, objectA, objectB, name);

            return ds;
        }

        private static IDataRow FindByCompositePk(IDataTable t, int a, string b)
        {
            for (int i = 0; i < t.Rows.Count; i++)
            {
                IDataRow r = t.Rows[i];
                if ((int)r["A"]! == a && (string)r["B"]! == b)
                {
                    return r;
                }
            }

            throw new InvalidOperationException("Row not found.");
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

    }
}
