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
            IObservableMergePolicy policy = ObservableMergePolicyFactory.Create(observableMergeOptions.MergeMode);

            if (policy.IsFullReload)
            {
                MergeObservableDataRowsWithoutPrimaryKeys(currentObservableDataTable, refreshedDataTable, observableMergeOptions);
                return;
            }

            List<string> primaryKeyColumnNames = currentObservableDataTable.GetPrimaryKeyColumnNames(observableMergeOptions);

            policy.ValidateAfterPrimaryKeyDiscovery(currentObservableDataTable, refreshedDataTable, observableMergeOptions, primaryKeyColumnNames.Count);

            if (primaryKeyColumnNames.Count == 0)
            {
                MergeObservableDataRowsWithoutPrimaryKeys(currentObservableDataTable, refreshedDataTable, observableMergeOptions);
                return;
            }

            if (policy.RequiresClientKeyCorrelation)
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

            if (policy.RejectDuplicateRefreshedPrimaryKeys)
            {
                ValidateNoDuplicatePrimaryKeys(refreshedDataTable, primaryKeyColumnNames, "refreshed");
            }

            Dictionary<string, IDataRow> refreshedIndex = RowIndexBuilder.BuildRowIndex(refreshedDataTable, primaryKeyColumnNames);

            Dictionary<Guid, IDataRow>? refreshedRowsByClientKey = null;
            Dictionary<Guid, IObservableDataRow>? currentRowsByClientKey = null;

            if (policy.RequiresClientKeyCorrelation
                && TableHasColumn(currentObservableDataTable.InnerDataTable, ClientKeyColumnName)
                && TableHasColumn(refreshedDataTable, ClientKeyColumnName))
            {
                refreshedRowsByClientKey = BuildRefreshedRowsByClientKey(refreshedDataTable, true);
                currentRowsByClientKey = BuildCurrentObservableRowsByClientKey(currentObservableDataTable, true);
            }

            HashSet<string> processedRefreshedPrimaryKeys = new HashSet<string>();

            MergeCurrentObservableDataTableRows(
                currentObservableDataTable,
                refreshedDataTable,
                observableMergeOptions,
                policy,
                primaryKeyColumnNames,
                refreshedIndex,
                processedRefreshedPrimaryKeys,
                refreshedRowsByClientKey);

            MergeNonProcessedDataRowsFromRefreshedDataTable(
                currentObservableDataTable,
                refreshedDataTable,
                observableMergeOptions,
                policy,
                refreshedIndex,
                processedRefreshedPrimaryKeys,
                currentRowsByClientKey);
        }
        #endregion

        #region Private Methods
        static void ValidateNoDuplicatePrimaryKeys(IDataTable dataTable, List<string> primaryKeyColumnNames, string tableRoleDescription)
        {
            HashSet<string> seen = new HashSet<string>();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                IDataRow row = dataTable.Rows[i];
                string key = row.CompilePrimaryKeyValue(primaryKeyColumnNames);

                if (seen.Contains(key))
                {
                    throw new InvalidOperationException(
                        "Duplicate primary key '" + key + "' detected in " + tableRoleDescription + " table '" + dataTable.TableName + "'.");
                }

                seen.Add(key);
            }
        }

        static bool TableHasColumn(IDataTable dataTable, string columnName)
        {
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
        IDataRow AddNewDataRowWithDefaultValuesToInnerTable(IObservableDataTable observableDataTable, IObservableMergeOptions observableMergeOptions)
        {
            IDataRow newDataRow = observableDataTable.InnerDataTable.AddNewRow();
            foreach (IColumnMetadata columnMetadata in observableDataTable.Columns)
            {
                newDataRow[columnMetadata.ColumnName] = observableMergeOptions.DataTypeDefaultValueProvider.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
            }

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
            IObservableMergePolicy policy,
            List<string> primaryKeyColumnNames,
            Dictionary<string, IDataRow> refreshedIndex,
            HashSet<string> processedRefreshedPrimaryKeys,
            Dictionary<Guid, IDataRow>? refreshedRowsByClientKey)
        {
            for (int i = currentObservableDataTable.Rows.Count - 1; i >= 0; i--)
            {
                IObservableDataRow observableDataRow = currentObservableDataTable.Rows[i];
                string currentPkValue = observableDataRow.CompilePrimaryKeyValue(primaryKeyColumnNames);

                // PostSave semantics: finalize deletes.
                // Deleted rows are removed from the current table regardless of whether the server returned
                // a corresponding row. Additionally, if a matching server row exists, it is considered
                // "processed" so it will not be re-added later.
                if (policy.Mode == MergeMode.PostSave && observableDataRow.InnerDataRow.DataRowState == DataRowState.Deleted)
                {
                    // Mark any matching refreshed row (by PK) as processed to prevent re-add.
                    if (refreshedIndex.ContainsKey(currentPkValue))
                    {
                        processedRefreshedPrimaryKeys.Add(currentPkValue);
                    }

                    IObservableDataRow removed = currentObservableDataTable.RemoveRowAt(i);
                    observableMergeOptions.ObservableDataSetMergeResult.DeletedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, removed));
                    continue;
                }

                IDataRow? refreshedDataRow = null;
                refreshedIndex.TryGetValue(currentPkValue, out refreshedDataRow);

                if (refreshedDataRow == null
                    && policy.RequiresClientKeyCorrelation
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
                    bool preserve = policy.PreserveRowWhenMissingFromRefreshed(observableDataRow.InnerDataRow.DataRowState);

                    if (preserve)
                    {
                    }
                    else if (observableMergeOptions.ExcludeTablesFromRowDeletion.Contains(currentObservableDataTable.TableName))
                    {
                    }
                    else
                    {
                        IObservableDataRow removed = currentObservableDataTable.RemoveRowAt(i);
                        observableMergeOptions.ObservableDataSetMergeResult.DeletedObservableDataRows.Add(
                            new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, removed));
                    }
                }
                else
                {
                    bool changed = observableDataRow.MergeWith(
                        refreshedDataRow,
                        currentObservableDataTable.TableName,
                        currentObservableDataTable.InnerDataTable.Columns,
                        observableMergeOptions);

                    if (changed)
                    {
                        observableMergeOptions.ObservableDataSetMergeResult.UpdatedObservableDataRows.Add(
                            new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, observableDataRow));
                    }

                    string refreshedPkValue = refreshedDataRow.CompilePrimaryKeyValue(primaryKeyColumnNames);
                    processedRefreshedPrimaryKeys.Add(refreshedPkValue);
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
            for (int i = 0; i < currentObservableDataTable.Rows.Count; i++)
            {
                observableMergeOptions.ObservableDataSetMergeResult.DeletedObservableDataRows.Add(
                    new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, currentObservableDataTable.Rows[i]));
            }

            for (int i = currentObservableDataTable.Rows.Count - 1; i >= 0; i--)
            {
                currentObservableDataTable.RemoveRowAt(i);
            }

            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedDataRow = refreshedDataTable.Rows[i];
                IDataRow newDataRow = AddNewDataRowWithDefaultValuesToInnerTable(currentObservableDataTable, observableMergeOptions);

                newDataRow.MergeWith(refreshedDataRow, currentObservableDataTable.TableName, currentObservableDataTable.Columns, observableMergeOptions);
                IObservableDataRow observableDataRow = currentObservableDataTable.AddRow(newDataRow);

                observableMergeOptions.ObservableDataSetMergeResult.AddedObservableDataRows.Add(
                    new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, observableDataRow));
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
            IObservableMergePolicy policy,
            Dictionary<string, IDataRow> refreshedIndex,
            HashSet<string> processedRefreshedPrimaryKeys,
            Dictionary<Guid, IObservableDataRow>? currentRowsByClientKey)
        {
            foreach (KeyValuePair<string, IDataRow> pair in refreshedIndex)
            {
                string pkValue = pair.Key;
                if (processedRefreshedPrimaryKeys.Contains(pkValue))
                {
                    continue;
                }

                IDataRow refreshedDataRow = pair.Value;

                if (policy.RequiresClientKeyCorrelation && currentRowsByClientKey != null)
                {
                    object? clientKeyValue;
                    refreshedDataRow.TryGetValue(ClientKeyColumnName, out clientKeyValue);

                    if (clientKeyValue is Guid clientKey && clientKey != Guid.Empty)
                    {
                        if (currentRowsByClientKey.ContainsKey(clientKey))
                        {
                            continue;
                        }
                    }
                }

                IDataRow newDataRow = AddNewDataRowWithDefaultValuesToInnerTable(currentObservableDataTable, observableMergeOptions);
                newDataRow.MergeWith(refreshedDataRow, currentObservableDataTable.TableName, currentObservableDataTable.Columns, observableMergeOptions);

                IObservableDataRow observableDataRow = currentObservableDataTable.AddRow(newDataRow);
                observableMergeOptions.ObservableDataSetMergeResult.AddedObservableDataRows.Add(
                    new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, observableDataRow));
            }
        }
        #endregion
    }
}
