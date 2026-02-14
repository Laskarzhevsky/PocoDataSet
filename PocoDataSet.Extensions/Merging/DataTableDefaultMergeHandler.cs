using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides data table default merge handler functionality
    /// </summary>
    public class DataTableDefaultMergeHandler : ITableMergeHandler
    {
        #region Public Methods
        /// <summary>
        /// Merges current table with refreshed table.
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        public void Merge(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            MergeContext context = new MergeContext(currentDataTable, refreshedDataTable, mergeOptions);

            ITableMergeStrategy strategy = TableMergeStrategyFactory.Create(context);
            strategy.Execute(this, context);
        }
        
        #region Merge Strategies
        sealed class MergeContext
        {
            public MergeContext(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
            {
                CurrentDataTable = currentDataTable;
                RefreshedDataTable = refreshedDataTable;
                MergeOptions = mergeOptions;
            }

            public IDataTable CurrentDataTable { get; private set; }

            public IDataTable RefreshedDataTable { get; private set; }

            public IMergeOptions MergeOptions { get; private set; }
        }

        interface ITableMergeStrategy
        {
            void Execute(DataTableDefaultMergeHandler handler, MergeContext context);
        }

        static class TableMergeStrategyFactory
        {
            public static ITableMergeStrategy Create(MergeContext context)
            {
                if (context.MergeOptions != null && context.MergeOptions.MergeMode == MergeMode.PostSave)
                {
                    return new PostSaveMergeStrategy();
                }

                return new RefreshMergeStrategy();
            }
        }

        sealed class RefreshMergeStrategy : ITableMergeStrategy
        {
            public void Execute(DataTableDefaultMergeHandler handler, MergeContext context)
            {
                handler.MergeRefresh(context.CurrentDataTable, context.RefreshedDataTable, context.MergeOptions);
            }
        }

        sealed class PostSaveMergeStrategy : ITableMergeStrategy
        {
            public void Execute(DataTableDefaultMergeHandler handler, MergeContext context)
            {
                handler.MergePostSave(context.CurrentDataTable, context.RefreshedDataTable, context.MergeOptions);
            }
        }
        #endregion

        #region Refresh Merge

        sealed class KeyedMergeContext
        {
            public KeyedMergeContext(
                IDataTable currentDataTable,
                IDataTable refreshedDataTable,
                IMergeOptions mergeOptions,
                List<string> primaryKeyColumnNames,
                Dictionary<string, IDataRow> refreshedIndex,
                HashSet<string> processedKeys)
            {
                CurrentDataTable = currentDataTable;
                RefreshedDataTable = refreshedDataTable;
                MergeOptions = mergeOptions;
                PrimaryKeyColumnNames = primaryKeyColumnNames;
                RefreshedIndex = refreshedIndex;
                ProcessedKeys = processedKeys;
            }

            public IDataTable CurrentDataTable { get; private set; }
            public IDataTable RefreshedDataTable { get; private set; }
            public IMergeOptions MergeOptions { get; private set; }
            public List<string> PrimaryKeyColumnNames { get; private set; }
            public Dictionary<string, IDataRow> RefreshedIndex { get; private set; }
            public HashSet<string> ProcessedKeys { get; private set; }
        }

        interface IKeyedMergeTemplate
        {
            void MergeExisting(DataTableDefaultMergeHandler handler, KeyedMergeContext context);
            void MergeNew(DataTableDefaultMergeHandler handler, KeyedMergeContext context);
        }

        sealed class RefreshKeyedMergeTemplate : IKeyedMergeTemplate
        {
            public void MergeExisting(DataTableDefaultMergeHandler handler, KeyedMergeContext context)
            {
                handler.MergeCurrentDataTableRows(
                    context.CurrentDataTable,
                    context.RefreshedDataTable,
                    context.MergeOptions,
                    context.PrimaryKeyColumnNames,
                    context.RefreshedIndex,
                    context.ProcessedKeys);
            }

            public void MergeNew(DataTableDefaultMergeHandler handler, KeyedMergeContext context)
            {
                handler.MergeNonProcessedDataRowsFromRefreshedDataTable(
                    context.CurrentDataTable,
                    context.RefreshedDataTable,
                    context.MergeOptions,
                    context.RefreshedIndex,
                    context.ProcessedKeys);
            }
        }

        Dictionary<string, IDataRow> BuildRowsByPrimaryKey(IDataTable dataTable, List<string> primaryKeyColumnNames, bool skipEmptyKey)
        {
            Dictionary<string, IDataRow> rowsByPrimaryKey = new Dictionary<string, IDataRow>(StringComparer.Ordinal);

            if (primaryKeyColumnNames == null || primaryKeyColumnNames.Count == 0)
            {
                return rowsByPrimaryKey;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                IDataRow row = dataTable.Rows[i];

                string pkValue;
                bool hasPrimaryKeyValue = RowIdentityResolver.TryGetPrimaryKeyValue(row, primaryKeyColumnNames, out pkValue);
                if (!hasPrimaryKeyValue)
                {
                    pkValue = string.Empty;
                }

                if (skipEmptyKey && string.IsNullOrEmpty(pkValue))
                {
                    continue;
                }

                if (!rowsByPrimaryKey.ContainsKey(pkValue))
                {
                    rowsByPrimaryKey.Add(pkValue, row);
                }
            }

            return rowsByPrimaryKey;
        }

        void MergeKeyedRefresh(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions, List<string> primaryKeyColumnNames)
        {
            Dictionary<string, IDataRow> refreshedIndex = refreshedDataTable.BuildPrimaryKeyIndex(primaryKeyColumnNames);
            HashSet<string> processedKeys = new HashSet<string>();

            KeyedMergeContext context = new KeyedMergeContext(
                currentDataTable,
                refreshedDataTable,
                mergeOptions,
                primaryKeyColumnNames,
                refreshedIndex,
                processedKeys);

            ExecuteKeyedMergeTemplate(new RefreshKeyedMergeTemplate(), context);
        }

        void ExecuteKeyedMergeTemplate(IKeyedMergeTemplate template, KeyedMergeContext context)
        {
            template.MergeExisting(this, context);
            template.MergeNew(this, context);
        }

        void MergeRefresh(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            List<string> currentDataTablePrimaryKeyColumnNames = mergeOptions.GetPrimaryKeyColumnNames(currentDataTable);
            if (currentDataTablePrimaryKeyColumnNames.Count == 0)
            {
                // Current table has no primary key
                MergeDataRowsWithoutPrimaryKeys(currentDataTable, refreshedDataTable, mergeOptions);
            }
            else
            {
                // Current table has primary key
                MergeKeyedRefresh(currentDataTable, refreshedDataTable, mergeOptions, currentDataTablePrimaryKeyColumnNames);
            }
        }
        #endregion

#endregion

        #region PostSave Merge
        void MergePostSave(IDataTable currentDataTable, IDataTable changesetDataTable, IMergeOptions mergeOptions)
        {
            List<string> primaryKeyColumnNames = mergeOptions.GetPrimaryKeyColumnNames(currentDataTable);

            Dictionary<string, IDataRow> currentRowsByPrimaryKey = BuildRowsByPrimaryKey(currentDataTable, primaryKeyColumnNames, true);



            Dictionary<Guid, IDataRow> currentRowsByClientKey = BuildClientKeyIndex(currentDataTable);

            for (int i = 0; i < changesetDataTable.Rows.Count; i++)
            {
                IDataRow changesetRow = changesetDataTable.Rows[i];

                if (changesetRow.DataRowState == DataRowState.Added)
                {
                    ApplyPostSaveRow(currentDataTable, changesetRow, primaryKeyColumnNames, currentRowsByPrimaryKey, currentRowsByClientKey, mergeOptions);
                    continue;
                }

                if (changesetRow.DataRowState == DataRowState.Modified)
                {
                    ApplyPostSaveRow(currentDataTable, changesetRow, primaryKeyColumnNames, currentRowsByPrimaryKey, currentRowsByClientKey, mergeOptions);
                    continue;
                }

                if (changesetRow.DataRowState == DataRowState.Deleted)
                {
                    ApplyPostSaveDelete(currentDataTable, changesetRow, primaryKeyColumnNames, currentRowsByPrimaryKey, currentRowsByClientKey, mergeOptions);
                    continue;
                }
            }
        }

        static Dictionary<Guid, IDataRow> BuildClientKeyIndex(IDataTable dataTable)
        {
            Dictionary<Guid, IDataRow> index = new Dictionary<Guid, IDataRow>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                IDataRow row = dataTable.Rows[i];
                Guid clientKey;
                if (TryGetClientKey(row, out clientKey))
                {
                    if (!index.ContainsKey(clientKey))
                    {
                        index.Add(clientKey, row);
                    }
                }
            }
            return index;
        }

        static bool TryGetClientKey(IDataRow row, out Guid clientKey)
        {
            clientKey = Guid.Empty;

            if (!row.ContainsKey(SpecialColumnNames.CLIENT_KEY))
            {
                return false;
            }

            object? value = row[SpecialColumnNames.CLIENT_KEY];
            if (value == null)
            {
                return false;
            }

            if (value is Guid)
            {
                clientKey = (Guid)value;
                return clientKey != Guid.Empty;
            }

            return false;
        }

        static void CopyAllValues(IDataRow targetRow, IDataRow sourceRow, IReadOnlyList<IColumnMetadata> targetColumns)
        {
            for (int c = 0; c < targetColumns.Count; c++)
            {
                string columnName = targetColumns[c].ColumnName;
                if (!sourceRow.ContainsKey(columnName))
                {
                    continue;
                }
                targetRow[columnName] = sourceRow[columnName];
            }
        }

        void ApplyPostSaveRow(IDataTable currentDataTable, IDataRow changesetRow, List<string> primaryKeyColumnNames,
            Dictionary<string, IDataRow> currentRowsByPrimaryKey,
            Dictionary<Guid, IDataRow> currentRowsByClientKey,
            IMergeOptions mergeOptions)
        {
            IDataRow? targetRow = null;

            // 1) Try match by primary key (works for updates, and for inserts if PK is already present in UI).
            if (primaryKeyColumnNames.Count > 0)
            {
                string pkValue;
                bool hasPrimaryKeyValue = RowIdentityResolver.TryGetPrimaryKeyValue(changesetRow, primaryKeyColumnNames, out pkValue);
                if (!hasPrimaryKeyValue)
                {
                    pkValue = string.Empty;
                }
                if (!string.IsNullOrEmpty(pkValue))
                {
                    currentRowsByPrimaryKey.TryGetValue(pkValue, out targetRow);
                }
            }

            // 2) Fallback: match by client key (identity inserts across the wire).
            if (targetRow == null)
            {
                Guid clientKey;
                if (TryGetClientKey(changesetRow, out clientKey))
                {
                    currentRowsByClientKey.TryGetValue(clientKey, out targetRow);
                }
            }

            bool isNewRowAddedToCurrent = false;
            if (targetRow == null)
            {
                // Current table did not contain the row; add it.
                IDataRow newRow = AddNewDataRowWithDefaultValuesToDataTable(currentDataTable, mergeOptions);
                targetRow = newRow;
                isNewRowAddedToCurrent = true;
            }

            CopyAllValues(targetRow, changesetRow, currentDataTable.Columns);

            // Server-confirmed baseline
            targetRow.AcceptChanges();

            if (isNewRowAddedToCurrent)
            {
                mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, targetRow));
            }
            else
            {
                mergeOptions.DataSetMergeResult.UpdatedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, targetRow));
            }
        }

        void ApplyPostSaveDelete(IDataTable currentDataTable, IDataRow changesetRow, List<string> primaryKeyColumnNames,
            Dictionary<string, IDataRow> currentRowsByPrimaryKey,
            Dictionary<Guid, IDataRow> currentRowsByClientKey,
            IMergeOptions mergeOptions)
        {
            IDataRow? targetRow = null;

            if (primaryKeyColumnNames.Count > 0)
            {
                string pkValue;
                bool hasPrimaryKeyValue = RowIdentityResolver.TryGetPrimaryKeyValue(changesetRow, primaryKeyColumnNames, out pkValue);
                if (!hasPrimaryKeyValue)
                {
                    pkValue = string.Empty;
                }
                if (!string.IsNullOrEmpty(pkValue))
                {
                    currentRowsByPrimaryKey.TryGetValue(pkValue, out targetRow);
                }
            }

            if (targetRow == null)
            {
                Guid clientKey;
                if (TryGetClientKey(changesetRow, out clientKey))
                {
                    currentRowsByClientKey.TryGetValue(clientKey, out targetRow);
                }
            }

            if (targetRow == null)
            {
                return;
            }

            for (int i = 0; i < currentDataTable.Rows.Count; i++)
            {
                if (ReferenceEquals(currentDataTable.Rows[i], targetRow))
                {
                    currentDataTable.RemoveRowAt(i);
                    mergeOptions.DataSetMergeResult.DeletedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, targetRow));
                    return;
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds new data row with default values to data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <returns>New row with default values added to data table</returns>
        private IDataRow AddNewDataRowWithDefaultValuesToDataTable(IDataTable dataTable, IMergeOptions mergeOptions)
        {
            // Create a detached row
            IDataRow newDataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(dataTable.Columns);

            // Add to table (still Detached)
            dataTable.AddRow(newDataRow);

            // This row represents server data, not a user "Added" row
            newDataRow.SetDataRowState(DataRowState.Unchanged);

            return newDataRow;
        }

        /// <summary>
        /// Merges current data table rows
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <param name="currentDataTablePrimaryKeyColumnNames">Current data table primary key column names</param>
        /// <param name="refreshedDataTableDataRowIndex">Refreshed data table data row index</param>
        /// <param name="primaryKeysOfMergedDataRows">Primary keys of merged data rows</param>
        void MergeCurrentDataTableRows(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions, List<string> currentDataTablePrimaryKeyColumnNames, Dictionary<string, IDataRow> refreshedDataTableDataRowIndex, HashSet<string> primaryKeysOfMergedDataRows)
        {
            // 1) Merge or delete existing rows
            for (int i = currentDataTable.Rows.Count - 1; i >= 0; i--)
            {
                IDataRow currentDataRow = currentDataTable.Rows[i];
                string primaryKeyValue;
                bool hasPrimaryKeyValue = RowIdentityResolver.TryGetPrimaryKeyValue(currentDataRow, currentDataTablePrimaryKeyColumnNames, out primaryKeyValue);
                if (!hasPrimaryKeyValue)
                {
                    primaryKeyValue = string.Empty;
                }

                IDataRow? refreshedDataRow;
                refreshedDataTableDataRowIndex.TryGetValue(primaryKeyValue, out refreshedDataRow);
                if (refreshedDataRow == null)
                {
                    bool shouldPreserveRow = false;

                    if (mergeOptions.MergeMode == MergeMode.Refresh)
                    {
                        // In refresh mode, preserve local client-side changes that have not been saved yet.
                        // Typical cases: Added row (new record), Modified row (edited record), Deleted row (pending delete).
                        if (currentDataRow.DataRowState != DataRowState.Unchanged)
                        {
                            shouldPreserveRow = true;
                        }
                    }

                    if (shouldPreserveRow)
                    {
                        // Keep current row intact
                    }
                    else if (mergeOptions.ExcludeTablesFromRowDeletion.Contains(currentDataTable.TableName))
                    {
                        // Keep current row intact
                    }
                    else
                    {
                        currentDataTable.RemoveRowAt(i);
                        mergeOptions.DataSetMergeResult.DeletedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, currentDataRow));
                    }
                }
                else
                {
                    bool changed = currentDataRow.MergeWith(refreshedDataRow, currentDataTable.TableName, currentDataTable.Columns, mergeOptions);
                    if (changed)
                    {
                        mergeOptions.DataSetMergeResult.UpdatedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, currentDataRow));
                    }

                    primaryKeysOfMergedDataRows.Add(primaryKeyValue);
                }
            }
        }

        /// <summary>
        /// Merges data row from refreshed data row
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>True if any value of the current data row changed, otherwise false</returns>
        private static bool MergeDataRowFromRefreshedDataRow(IDataRow currentDataRow, IDataRow refreshedDataRow, IReadOnlyList<IColumnMetadata> listOfColumnMetadata, IMergeOptions mergeOptions)
        {
            // Refresh must never overwrite local edits.
            if (mergeOptions != null && mergeOptions.MergeMode == MergeMode.Refresh)
            {
                if (currentDataRow.DataRowState != DataRowState.Unchanged)
                {
                    return false;
                }
            }

            if (currentDataRow.DataRowState == DataRowState.Deleted)
            {
                // Keep local delete during Refresh merge; do not attempt to overwrite or AcceptChanges.
                return false;
            }

            bool changed = false;
            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                string columnName = listOfColumnMetadata[i].ColumnName;
                object? oldValue = currentDataRow[columnName];
                object? newValue = refreshedDataRow[columnName];
                if (DataFieldValuesComparer.FieldValuesEqual(oldValue, newValue))
                {
                    continue;
                }

                currentDataRow[columnName] = newValue;
                changed = true;
            }

            if (changed)
            {
                currentDataRow.AcceptChanges(); // refreshed values become baseline
            }

            return changed;
        }


        /// <summary>
        /// Merges data rows without primary keys
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        void MergeDataRowsWithoutPrimaryKeys(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            // Track deletions BEFORE clearing the table
            for (int i = 0; i < currentDataTable.Rows.Count; i++)
            {
                mergeOptions.DataSetMergeResult.DeletedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, currentDataTable.Rows[i]));
            }

            // Clear current rows
            currentDataTable.RemoveAllRows();

            // Add all refreshed rows as new rows
            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedRow = refreshedDataTable.Rows[i];
                IDataRow newDataRow = AddNewDataRowWithDefaultValuesToDataTable(currentDataTable, mergeOptions);

                MergeDataRowFromRefreshedDataRow(newDataRow, refreshedRow, currentDataTable.Columns, mergeOptions);
                mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, newDataRow));
            }
        }

        /// <summary>
        /// Merges non-processed data rows from refreshed data table
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <param name="refreshedDataTableDataRowIndex">Refreshed data table data row index</param>
        /// <param name="primaryKeysOfMergedDataRows">Primary keys of merged data rows</param>
        void MergeNonProcessedDataRowsFromRefreshedDataTable(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions, Dictionary<string, IDataRow> refreshedDataTableDataRowIndex, HashSet<string> primaryKeysOfMergedDataRows)
        {
            foreach (KeyValuePair<string, IDataRow> keyValuePair in refreshedDataTableDataRowIndex)
            {
                string primaryKeyValue = keyValuePair.Key;
                if (primaryKeysOfMergedDataRows.Contains(primaryKeyValue))
                {
                    continue;
                }

                IDataRow newDataRow = AddNewDataRowWithDefaultValuesToDataTable(currentDataTable, mergeOptions);
                IDataRow refreshedDataRow = keyValuePair.Value;

                newDataRow.MergeWith(refreshedDataRow, currentDataTable.TableName, currentDataTable.Columns, mergeOptions);
                mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, newDataRow));
            }
        }

        #endregion
    }
}
