using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Creates a "delta changeset" dataset.
        /// - Added rows: full row values (same as CreateChangeset).
        /// - Modified rows: only primary key columns + __ClientKey (if present) + changed columns.
        /// - Deleted rows: only primary key columns + __ClientKey (if present).
        /// </summary>
        /// <param name="dataSet">Source data set</param>
        /// <returns>Delta changeset data set, or null if source is null</returns>
        public static IDataSet? CreateChangeset(this IDataSet? dataSet)
        {
            if (dataSet == null)
            {
                return null;
            }

            IDataSet changeset = DataSetFactory.CreateDataSet();

            foreach (KeyValuePair<string, IDataTable> keyValuePair in dataSet.Tables)
            {
                string tableName = keyValuePair.Key;
                IDataTable sourceTable = keyValuePair.Value;

                // Collect changed rows
                List<IDataRow> changedRows = new List<IDataRow>();
                for (int i = 0; i < sourceTable.Rows.Count; i++)
                {
                    IDataRow row = sourceTable.Rows[i];
                    if (row.DataRowState == DataRowState.Added ||
                        row.DataRowState == DataRowState.Modified ||
                        row.DataRowState == DataRowState.Deleted)
                    {
                        changedRows.Add(row);
                    }
                }

                if (changedRows.Count == 0)
                {
                    continue;
                }

                // Create target table with same schema
                IDataTable targetTable = changeset.AddNewTable(tableName);
                targetTable.AddColumns(sourceTable.Columns);

                // Preserve primary keys
                if (sourceTable.PrimaryKeys != null && sourceTable.PrimaryKeys.Count > 0)
                {
                    List<string> primaryKeys = new List<string>();
                    for (int i = 0; i < sourceTable.PrimaryKeys.Count; i++)
                    {
                        primaryKeys.Add(sourceTable.PrimaryKeys[i]);
                    }

                    targetTable.SetPrimaryKeys(primaryKeys);
                }

                // Copy rows as deltas
                for (int i = 0; i < changedRows.Count; i++)
                {
                    IDataRow sourceRow = changedRows[i];

                    IDataRow targetRow;

                    if (sourceRow.DataRowState == DataRowState.Added)
                    {
                        // Added rows: full row values (insert payload)
                        targetRow = DataRowExtensions.CreateRowFromColumns(sourceTable.Columns);
                    }
                    else
                    {
                        // Modified / Deleted rows: floating row that contains ONLY explicitly provided fields.
                        // Missing fields mean "not provided" (distinct from a provided null).
                        targetRow = DataRowExtensions.CreateFloatingRow(sourceTable.Columns.Count);
                    }

                    List<string> columnsToCopy = GetColumnsToCopy(sourceTable, sourceRow);

                    for (int c = 0; c < columnsToCopy.Count; c++)
                    {
                        string columnName = columnsToCopy[c];

                        object? value = null;

                        // 1) Prefer current/provided value
                        if (sourceRow.TryGetValue(columnName, out value))
                        {
                            targetRow[columnName] = value;
                            continue;
                        }

                        // 2) Fallback to original value (important for PK / __ClientKey on floating rows)
                        if (sourceRow.TryGetOriginalValue(columnName, out value))
                        {
                            targetRow[columnName] = value;
                            continue;
                        }

                        // 3) Neither exists -> do not include the field
                    }

                    // IMPORTANT:
                    // DataTable.AddRow(IDataRow) disallows adding a row in Deleted state.
                    // For Deleted rows we add as Loaded/Unchanged first, then mark Deleted via DeleteRow.
                    if (sourceRow.DataRowState == DataRowState.Deleted)
                    {
                        targetTable.AddLoadedRow(targetRow);
                        targetTable.DeleteRow(targetRow);
                    }
                    else
                    {
                        // Added or Modified: preserve state explicitly, then add
                        targetRow.SetDataRowState(sourceRow.DataRowState);
                        targetTable.AddRow(targetRow);
                    }
                }
            }

            return changeset;
        }
        #endregion

        #region Private Methods
        static List<string> GetColumnsToCopy(IDataTable sourceTable, IDataRow sourceRow)
        {
            List<string> columnsToCopy = new List<string>();

            // Primary keys (for Modified/Deleted)
            if (sourceRow.DataRowState == DataRowState.Modified ||
                sourceRow.DataRowState == DataRowState.Deleted)
            {
                if (sourceTable.PrimaryKeys != null && sourceTable.PrimaryKeys.Count > 0)
                {
                    for (int i = 0; i < sourceTable.PrimaryKeys.Count; i++)
                    {
                        string pk = sourceTable.PrimaryKeys[i];
                        if (!columnsToCopy.Contains(pk))
                        {
                            columnsToCopy.Add(pk);
                        }
                    }
                }

                // __ClientKey (optional)
                if (sourceTable.ContainsColumn(SpecialColumnNames.CLIENT_KEY) && !columnsToCopy.Contains(SpecialColumnNames.CLIENT_KEY))
                {
                    columnsToCopy.Add(SpecialColumnNames.CLIENT_KEY);
                }
            }

            if (sourceRow.DataRowState == DataRowState.Added)
            {
                // For inserts we keep the existing behavior: include full row values.
                for (int i = 0; i < sourceTable.Columns.Count; i++)
                {
                    string columnName = sourceTable.Columns[i].ColumnName;
                    if (!columnsToCopy.Contains(columnName))
                    {
                        columnsToCopy.Add(columnName);
                    }
                }

                return columnsToCopy;
            }

            if (sourceRow.DataRowState == DataRowState.Modified)
            {
                // Add only changed columns.
                for (int i = 0; i < sourceTable.Columns.Count; i++)
                {
                    string columnName = sourceTable.Columns[i].ColumnName;

                    // skip keys / client key (already included above)
                    if (columnsToCopy.Contains(columnName))
                    {
                        continue;
                    }

                    object? currentValue = null;
                    bool hasCurrent = sourceRow.TryGetValue(columnName, out currentValue);

                    object? originalValue = null;
                    bool hasOriginal = sourceRow.TryGetOriginalValue(columnName, out originalValue);

                    // If we can't find current, nothing to send.
                    if (!hasCurrent)
                    {
                        continue;
                    }

                    // If we don't have original baseline (unusual for Modified), be conservative and treat as changed.
                    if (!hasOriginal)
                    {
                        columnsToCopy.Add(columnName);
                        continue;
                    }

                    if (!AreEqual(originalValue, currentValue))
                    {
                        columnsToCopy.Add(columnName);
                    }
                }
            }

            // Deleted: only keys + __ClientKey
            return columnsToCopy;
        }

        static bool AreEqual(object? left, object? right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.Equals(right);
        }
        #endregion
    }
}
