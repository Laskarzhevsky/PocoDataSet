using System;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Provides observable data table default merge handler functionality
    /// </summary>
    public class ObservableDataTableDefaultMergeHandler : IObservableDataTableMergeHandler
    {
        #region Private Constants
        // Correlation column used by PostSave merge to map client-side Added rows to server-returned rows
        const string ClientKeyColumnName = SpecialColumnNames.CLIENT_KEY;
        #endregion

        #region Public Methods
        /// <summary>
        /// Merges current observable data table with refreshed data table
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        public void Merge(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            List<string> currentObservableDataTablePrimaryKeyColumnNames = currentObservableDataTable.GetPrimaryKeyColumnNames(observableMergeOptions);
            if (currentObservableDataTablePrimaryKeyColumnNames.Count == 0)
            {
                // Current table has no primary key (treat as reload/replace).
                MergeObservableDataRowsWithoutPrimaryKeys(currentObservableDataTable, refreshedDataTable, observableMergeOptions);
                return;
            }

            // Current table has primary key
            // PostSave requires __ClientKey for safe correlation (identity/rowversion propagation)
            if (observableMergeOptions.MergeMode == MergeMode.PostSave)
            {
                if (!TableHasColumn(currentObservableDataTable.InnerDataTable, ClientKeyColumnName))
                {
                    throw new InvalidOperationException(
                        "MergeMode.PostSave requires column '" + ClientKeyColumnName + "' in current table '" + currentObservableDataTable.TableName + "'.");
                }

                if (!TableHasColumn(refreshedDataTable, ClientKeyColumnName))
                {
                    throw new InvalidOperationException(
                        "MergeMode.PostSave requires column '" + ClientKeyColumnName + "' in refreshed table '" + refreshedDataTable.TableName + "'.");
                }
            }

            Dictionary<string, IDataRow> refreshedDataTableDataRowIndex = refreshedDataTable.BuildDataRowIndex(currentObservableDataTablePrimaryKeyColumnNames);

            // PostSave correlation: build row maps by __ClientKey (if present in both tables)
            Dictionary<Guid, IDataRow>? refreshedRowsByClientKey = null;
            Dictionary<Guid, IObservableDataRow>? currentRowsByClientKey = null;

            if (observableMergeOptions.MergeMode == MergeMode.PostSave
                && TableHasColumn(currentObservableDataTable.InnerDataTable, ClientKeyColumnName)
                && TableHasColumn(refreshedDataTable, ClientKeyColumnName))
            {
                refreshedRowsByClientKey = BuildRefreshedRowsByClientKey(refreshedDataTable, true);
                currentRowsByClientKey = BuildCurrentObservableRowsByClientKey(currentObservableDataTable, true);
            }

            HashSet<string> primaryKeysOfMergedRefreshedRows = new HashSet<string>();

            MergeCurrentObservableDataTableRows(
                currentObservableDataTable,
                refreshedDataTable,
                observableMergeOptions,
                currentObservableDataTablePrimaryKeyColumnNames,
                refreshedDataTableDataRowIndex,
                primaryKeysOfMergedRefreshedRows,
                refreshedRowsByClientKey);

            MergeNonProcessedDataRowsFromRefreshedDataTable(
                currentObservableDataTable,
                refreshedDataTable,
                observableMergeOptions,
                refreshedDataTableDataRowIndex,
                primaryKeysOfMergedRefreshedRows,
                currentRowsByClientKey);
        }
        #endregion

        #region Private Methods
        static bool TableHasColumn(IDataTable dataTable, string columnName)
        {
            // IDataTable.Columns is IList<IColumnMetadata>
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (dataTable.Columns[i].ColumnName == columnName)
                {
                    return true;
                }
            }

            return false;
        }

        static Dictionary<Guid, IDataRow> BuildRefreshedRowsByClientKey(IDataTable refreshedDataTable, bool requireClientKey)
        {
            Dictionary<Guid, IDataRow> index = new Dictionary<Guid, IDataRow>();

            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow row = refreshedDataTable.Rows[i];

                object? value;
                row.TryGetValue(ClientKeyColumnName, out value);

                if (value == null)
                {
                    if (requireClientKey)
                    {
                        throw new InvalidOperationException(
                            "MergeMode.PostSave requires non-null '" + ClientKeyColumnName + "' values in refreshed table '" + refreshedDataTable.TableName + "'.");
                    }

                    continue;
                }

                if (!(value is Guid g) || g == Guid.Empty)
                {
                    if (requireClientKey)
                    {
                        throw new InvalidOperationException(
                            "MergeMode.PostSave requires non-empty '" + ClientKeyColumnName + "' values in refreshed table '" + refreshedDataTable.TableName + "'.");
                    }

                    continue;
                }

                if (index.ContainsKey(g))
                {
                    throw new InvalidOperationException(
                        "Duplicate " + ClientKeyColumnName + " '" + g.ToString() + "' detected in refreshed table '" + refreshedDataTable.TableName + "'.");
                }

                index[g] = row;
            }

            return index;
        }

        static Dictionary<Guid, IObservableDataRow> BuildCurrentObservableRowsByClientKey(IObservableDataTable currentObservableDataTable, bool requireClientKey)
        {
            Dictionary<Guid, IObservableDataRow> index = new Dictionary<Guid, IObservableDataRow>();

            for (int i = 0; i < currentObservableDataTable.Rows.Count; i++)
            {
                IObservableDataRow row = currentObservableDataTable.Rows[i];

                object? value;
                row.InnerDataRow.TryGetValue(ClientKeyColumnName, out value);

                if (value == null)
                {
                    if (requireClientKey)
                    {
                        throw new InvalidOperationException(
                            "MergeMode.PostSave requires non-null '" + ClientKeyColumnName + "' values in current table '" + currentObservableDataTable.TableName + "'.");
                    }

                    continue;
                }

                if (!(value is Guid g) || g == Guid.Empty)
                {
                    if (requireClientKey)
                    {
                        throw new InvalidOperationException(
                            "MergeMode.PostSave requires non-empty '" + ClientKeyColumnName + "' values in current table '" + currentObservableDataTable.TableName + "'.");
                    }

                    continue;
                }

                if (index.ContainsKey(g))
                {
                    throw new InvalidOperationException(
                        "Duplicate " + ClientKeyColumnName + " '" + g.ToString() + "' detected in current table '" + currentObservableDataTable.TableName + "'.");
                }

                index[g] = row;
            }

            return index;
        }

        /// <summary>
        /// Adds new data row with default values to the inner data table and returns the row.
        /// The row is immediately put into Unchanged state, because it represents server-origin data.
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <returns>New row with default values added to data table</returns>
        private IDataRow AddNewDataRowWithDefaultValuesToInnerTable(IObservableDataTable observableDataTable, IObservableMergeOptions observableMergeOptions)
        {
            IDataRow newDataRow = observableDataTable.InnerDataTable.AddNewRow();
            foreach (IColumnMetadata columnMetadata in observableDataTable.Columns)
            {
                newDataRow[columnMetadata.ColumnName] = observableMergeOptions.DataTypeDefaultValueProvider.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
            }

            // This row is being created from refreshed/server data, not from user entry.
            newDataRow.AcceptChanges();

            return newDataRow;
        }

        /// <summary>
        /// Merges current observable data table rows
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <param name="currentObservableDataTablePrimaryKeyColumnNames">Current observable data table primary key column names</param>
        /// <param name="refreshedDataTableDataRowIndex">Refreshed data table data row index</param>
        /// <param name="primaryKeysOfMergedRefreshedRows">Primary keys of refreshed rows that were merged</param>
        /// <param name="refreshedRowsByClientKey">Optional refreshed-row index by __ClientKey for PostSave correlation</param>
        void MergeCurrentObservableDataTableRows(
            IObservableDataTable currentObservableDataTable,
            IDataTable refreshedDataTable,
            IObservableMergeOptions observableMergeOptions,
            List<string> currentObservableDataTablePrimaryKeyColumnNames,
            Dictionary<string, IDataRow> refreshedDataTableDataRowIndex,
            HashSet<string> primaryKeysOfMergedRefreshedRows,
            Dictionary<Guid, IDataRow>? refreshedRowsByClientKey)
        {
            for (int i = currentObservableDataTable.Rows.Count - 1; i >= 0; i--)
            {
                IObservableDataRow observableDataRow = currentObservableDataTable.Rows[i];
                string observableDataRowPrimaryKeyValue = observableDataRow.CompilePrimaryKeyValue(currentObservableDataTablePrimaryKeyColumnNames);

                IDataRow? refreshedDataRow = null;
                refreshedDataTableDataRowIndex.TryGetValue(observableDataRowPrimaryKeyValue, out refreshedDataRow);

                // PostSave correlation: when PK doesn't match yet (e.g., identity assigned by DB),
                // try to match by __ClientKey and propagate server-generated values into the existing row.
                if (refreshedDataRow == null
                    && observableMergeOptions.MergeMode == MergeMode.PostSave
                    && refreshedRowsByClientKey != null)
                {
                    object? clientKeyValue;
                    observableDataRow.InnerDataRow.TryGetValue(ClientKeyColumnName, out clientKeyValue);

                    if (clientKeyValue is Guid clientKey && clientKey != Guid.Empty)
                    {
                        refreshedRowsByClientKey.TryGetValue(clientKey, out refreshedDataRow);
                    }
                }

                if (refreshedDataRow == null)
                {
                    bool shouldPreserveRow = false;

                    // Refresh must preserve local pending changes (Added/Modified/Deleted).
                    if (observableMergeOptions.MergeMode == MergeMode.Refresh)
                    {
                        if (observableDataRow.InnerDataRow.DataRowState != DataRowState.Unchanged)
                        {
                            shouldPreserveRow = true;
                        }
                    }

                    // PostSave should never treat "missing from refreshed" as deletion unless you explicitly opt into it.
                    if (observableMergeOptions.MergeMode == MergeMode.PostSave)
                    {
                        shouldPreserveRow = true;
                    }

                    if (shouldPreserveRow)
                    {
                        // Keep row intact
                    }
                    else if (observableMergeOptions.ExcludeTablesFromRowDeletion.Contains(currentObservableDataTable.TableName))
                    {
                        // Keep row intact
                    }
                    else
                    {
                        IObservableDataRow removedObservableDataRow = currentObservableDataTable.RemoveRowAt(i);
                        observableMergeOptions.ObservableDataSetMergeResult.DeletedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, removedObservableDataRow));
                    }
                }
                else
                {
                    bool changed = observableDataRow.MergeWith(refreshedDataRow, currentObservableDataTable.TableName, currentObservableDataTable.InnerDataTable.Columns, observableMergeOptions);
                    if (changed)
                    {
                        observableMergeOptions.ObservableDataSetMergeResult.UpdatedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, observableDataRow));
                    }

                    // IMPORTANT: mark the REFRESHED row as processed (not the current row's PK),
                    // because PostSave correlation can merge Id=0 row with Id=10 refreshed row.
                    string refreshedPrimaryKeyValue = refreshedDataRow.CompilePrimaryKeyValue(currentObservableDataTablePrimaryKeyColumnNames);
                    primaryKeysOfMergedRefreshedRows.Add(refreshedPrimaryKeyValue);
                }
            }
        }

        /// <summary>
        /// Reload semantics for tables without primary keys:
        /// remove all current rows and add all refreshed rows.
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        void MergeObservableDataRowsWithoutPrimaryKeys(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            // Track deletions BEFORE clearing the table
            for (int i = 0; i < currentObservableDataTable.Rows.Count; i++)
            {
                observableMergeOptions.ObservableDataSetMergeResult.DeletedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, currentObservableDataTable.Rows[i]));
            }

            // Clear current rows
            for (int i = currentObservableDataTable.Rows.Count - 1; i >= 0; i--)
            {
                currentObservableDataTable.RemoveRowAt(i);
            }

            // Add all refreshed rows as new rows
            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedDataRow = refreshedDataTable.Rows[i];
                IDataRow newDataRow = AddNewDataRowWithDefaultValuesToInnerTable(currentObservableDataTable, observableMergeOptions);

                // Copy refreshed values (this will AcceptChanges in Refresh/PostSave via the row handler).
                newDataRow.MergeWith(refreshedDataRow, currentObservableDataTable.TableName, currentObservableDataTable.Columns, observableMergeOptions);
                IObservableDataRow observableDataRow = currentObservableDataTable.AddRow(newDataRow);
                observableMergeOptions.ObservableDataSetMergeResult.AddedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, observableDataRow));
            }
        }

        /// <summary>
        /// Adds rows from refreshed table which had not been merged already
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <param name="refreshedDataTableDataRowIndex">Refreshed data table data row index</param>
        /// <param name="primaryKeysOfMergedRefreshedRows">Primary keys of refreshed rows that were merged</param>
        /// <param name="currentRowsByClientKey">Optional current-row index by __ClientKey for PostSave correlation</param>
        void MergeNonProcessedDataRowsFromRefreshedDataTable(
            IObservableDataTable currentObservableDataTable,
            IDataTable refreshedDataTable,
            IObservableMergeOptions observableMergeOptions,
            Dictionary<string, IDataRow> refreshedDataTableDataRowIndex,
            HashSet<string> primaryKeysOfMergedRefreshedRows,
            Dictionary<Guid, IObservableDataRow>? currentRowsByClientKey)
        {
            foreach (KeyValuePair<string, IDataRow> keyValuePair in refreshedDataTableDataRowIndex)
            {
                string primaryKeyValue = keyValuePair.Key;
                if (primaryKeysOfMergedRefreshedRows.Contains(primaryKeyValue))
                {
                    continue;
                }

                IDataRow refreshedDataRow = keyValuePair.Value;

                // PostSave: if this refreshed row correlates to an existing current row by __ClientKey,
                // it should have been merged already. Do not add it as a new row (prevents duplicates).
                if (observableMergeOptions.MergeMode == MergeMode.PostSave && currentRowsByClientKey != null)
                {
                    object? clientKeyValue;
                    refreshedDataRow.TryGetValue(ClientKeyColumnName, out clientKeyValue);

                    if (clientKeyValue is Guid clientKey && clientKey != Guid.Empty)
                    {
                        if (currentRowsByClientKey.ContainsKey(clientKey))
                        {
                            // This is a correlated row; skip adding.
                            // If it wasn't merged earlier, that indicates missing correlation handling or inconsistent data.
                            continue;
                        }
                    }
                }

                IDataRow newDataRow = AddNewDataRowWithDefaultValuesToInnerTable(currentObservableDataTable, observableMergeOptions);

                // Copy refreshed values (row handler will commit baseline).
                newDataRow.MergeWith(refreshedDataRow, currentObservableDataTable.TableName, currentObservableDataTable.Columns, observableMergeOptions);
                IObservableDataRow observableDataRow = currentObservableDataTable.AddRow(newDataRow);
                observableMergeOptions.ObservableDataSetMergeResult.AddedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, observableDataRow));
            }
        }
        #endregion
    }
}
