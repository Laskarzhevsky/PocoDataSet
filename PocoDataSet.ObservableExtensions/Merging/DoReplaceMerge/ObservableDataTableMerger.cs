using System;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.DoReplaceMerge
{
    /// <summary>
    /// Provides observable data table default merge handler functionality
    /// </summary>
    internal sealed class ObservableDataTableMerger
    {
        #region Private Constants
        // Correlation column used by PostSave merge to map client-side Added rows to server-returned rows
        const string ClientKeyColumnName = SpecialColumnNames.CLIENT_KEY;
        #endregion

        #region Public Methods

                public void Merge(
            IObservableDataTable currentObservableDataTable,
            IDataTable refreshedDataTable,
            IObservableMergeOptions observableMergeOptions)
        {
            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            ObservableDataRowMerger rowMerger = new ObservableDataRowMerger();
            MergeInternal(currentObservableDataTable, refreshedDataTable, observableMergeOptions, rowMerger);
        }
        #endregion

        #region Private Methods
        void MergeInternal(
            IObservableDataTable currentObservableDataTable,
            IDataTable refreshedDataTable,
            IObservableMergeOptions observableMergeOptions,
            IObservableRowMerger rowMerger)
        {

            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            ValidateSchemaCompatibilityForReplace(currentObservableDataTable, refreshedDataTable);

            MergeObservableDataRowsWithoutPrimaryKeys(
                currentObservableDataTable,
                refreshedDataTable,
                observableMergeOptions,
                "Replace",
                rowMerger);
        
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
            string mergeName,
            bool isPostSaveMerge,
            System.Func<DataRowState, bool> preserveRowWhenMissingFromRefreshed,
            IObservableRowMerger rowMerger,
            List<string> primaryKeyColumnNames,
            Dictionary<string, IDataRow> refreshedIndex,
            HashSet<string> processedRefreshedPrimaryKeys,
            Dictionary<Guid, IDataRow>? refreshedRowsByClientKey,
            bool requiresClientKeyCorrelation)
        {
            for (int i = currentObservableDataTable.Rows.Count - 1; i >= 0; i--)
            {
                IObservableDataRow observableDataRow = currentObservableDataTable.Rows[i];
                string currentPkValue;

                bool hasPrimaryKeyValue = RowIdentityResolver.TryGetPrimaryKeyValue(observableDataRow.InnerDataRow, primaryKeyColumnNames, out currentPkValue);

                if (!hasPrimaryKeyValue)

                {

                    currentPkValue = string.Empty;

                }

                // PostSave semantics: finalize deletes.
                // Deleted rows are removed from the current table regardless of whether the server returned
                // a corresponding row. Additionally, if a matching server row exists, it is considered
                // "processed" so it will not be re-added later.
                if (isPostSaveMerge && observableDataRow.InnerDataRow.DataRowState == DataRowState.Deleted)
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
                    && requiresClientKeyCorrelation
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
                    bool preserve = preserveRowWhenMissingFromRefreshed(observableDataRow.InnerDataRow.DataRowState);

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
                    bool changed = rowMerger.MergeObservableRow(
                        currentObservableDataTable.TableName,
                        observableDataRow,
                        refreshedDataRow,
                        currentObservableDataTable.InnerDataTable.Columns,
                        observableMergeOptions);

                    if (changed)
                    {
                        observableMergeOptions.ObservableDataSetMergeResult.UpdatedObservableDataRows.Add(
                            new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, observableDataRow));
                    }

                    string refreshedPkValue;
                    bool hasPrimaryKeyValueForRefreshedRow = RowIdentityResolver.TryGetPrimaryKeyValue(refreshedDataRow, primaryKeyColumnNames, out refreshedPkValue);
                    if (!hasPrimaryKeyValueForRefreshedRow)
                    {
                        refreshedPkValue = string.Empty;
                    }

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
        internal void MergeObservableDataRowsWithoutPrimaryKeys(
            IObservableDataTable currentObservableDataTable,
            IDataTable refreshedDataTable,
            IObservableMergeOptions observableMergeOptions,
            string mergeName,
            IObservableRowMerger rowMerger)
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

                rowMerger.MergeDataRow(currentObservableDataTable.TableName, newDataRow, refreshedDataRow, currentObservableDataTable.Columns, observableMergeOptions);
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
            bool requiresClientKeyCorrelation,
            IObservableRowMerger rowMerger,
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

                if (requiresClientKeyCorrelation && currentRowsByClientKey != null)
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
                rowMerger.MergeDataRow(currentObservableDataTable.TableName, newDataRow, refreshedDataRow, currentObservableDataTable.Columns, observableMergeOptions);

                IObservableDataRow observableDataRow = currentObservableDataTable.AddRow(newDataRow);
                observableMergeOptions.ObservableDataSetMergeResult.AddedObservableDataRows.Add(
                    new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, observableDataRow));
            }
        }
        
        private static void ValidateSchemaCompatibilityForReplace(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable)
        {
            if (currentObservableDataTable == null)
            {
                throw new ArgumentNullException(nameof(currentObservableDataTable));
            }

            if (refreshedDataTable == null)
            {
                throw new ArgumentNullException(nameof(refreshedDataTable));
            }

            // Policy A: current schema is authoritative. Extra columns in refreshed are ignored.
            // Missing columns in refreshed are allowed (values become null/default on replaced rows).
            // BUT if a column exists in both, the data type must match to avoid silent corruption.
            for (int i = 0; i < currentObservableDataTable.Columns.Count; i++)
            {
                IColumnMetadata currentColumn = currentObservableDataTable.Columns[i];
                IColumnMetadata refreshedColumn = FindColumnByName(refreshedDataTable, currentColumn.ColumnName);

                if (refreshedColumn != null)
                {
                    if (!string.Equals(currentColumn.DataType, refreshedColumn.DataType, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "Replace merge requires matching column data types for column '" + currentColumn.ColumnName +
                            "'. Current type '" + currentColumn.DataType + "', refreshed type '" + refreshedColumn.DataType + "'.");
                    }
                }
            }
        }

        private static IColumnMetadata FindColumnByName(IDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (string.Equals(table.Columns[i].ColumnName, columnName, StringComparison.Ordinal))
                {
                    return table.Columns[i];
                }
            }

            return null;
        }

#endregion
    }
}
