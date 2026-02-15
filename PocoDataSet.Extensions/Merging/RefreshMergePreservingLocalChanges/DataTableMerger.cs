using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.RefreshMergePreservingLocalChanges
{
    /// <summary>
    /// Refresh-preserving table merge: reconciles refreshed data with the current table using primary keys,
    /// while preserving local pending changes (Added/Modified/Deleted).
    /// </summary>
    public sealed class DataTableMerger
    {
        public void Merge(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            if (currentDataTable == null)
            {
                throw new ArgumentNullException(nameof(currentDataTable));
            }

            if (refreshedDataTable == null)
            {
                throw new ArgumentNullException(nameof(refreshedDataTable));
            }

            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            List<string> primaryKeyColumnNames = mergeOptions.GetPrimaryKeyColumnNames(currentDataTable);
            if (primaryKeyColumnNames.Count == 0)
            {
                MergeWithoutPrimaryKeys(currentDataTable, refreshedDataTable, mergeOptions);
                return;
            }

            Dictionary<string, IDataRow> refreshedIndex = refreshedDataTable.BuildPrimaryKeyIndex(primaryKeyColumnNames);
            HashSet<string> mergedPrimaryKeys = new HashSet<string>(StringComparer.Ordinal);

            MergeExistingRows(currentDataTable, refreshedDataTable, mergeOptions, primaryKeyColumnNames, refreshedIndex, mergedPrimaryKeys);
            AddNonProcessedRefreshedRows(currentDataTable, mergeOptions, refreshedIndex, mergedPrimaryKeys);
        }

        private static void MergeWithoutPrimaryKeys(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            // Track deletions BEFORE clearing the table.
            for (int i = 0; i < currentDataTable.Rows.Count; i++)
            {
                mergeOptions.DataSetMergeResult.DeletedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, currentDataTable.Rows[i]));
            }

            currentDataTable.RemoveAllRows();

            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedRow = refreshedDataTable.Rows[i];
                IDataRow newRow = AddNewDataRowWithDefaultValuesToDataTable(currentDataTable);

                bool changed = newRow.DoRefreshMergePreservingLocalChanges(refreshedRow, currentDataTable.TableName, currentDataTable.Columns, mergeOptions);
                // new row was empty; treat as added regardless.
                mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, newRow));
            }
        }

        private static void MergeExistingRows(
            IDataTable currentDataTable,
            IDataTable refreshedDataTable,
            IMergeOptions mergeOptions,
            List<string> primaryKeyColumnNames,
            Dictionary<string, IDataRow> refreshedIndex,
            HashSet<string> mergedPrimaryKeys)
        {
            for (int i = currentDataTable.Rows.Count - 1; i >= 0; i--)
            {
                IDataRow currentRow = currentDataTable.Rows[i];

                string pkValue;
                bool ok = RowIdentityResolver.TryGetPrimaryKeyValue(currentRow, primaryKeyColumnNames, out pkValue);
                if (!ok)
                {
                    pkValue = string.Empty;
                }

                IDataRow? refreshedRow;
                refreshedIndex.TryGetValue(pkValue, out refreshedRow);

                if (refreshedRow == null)
                {
                    bool shouldPreserveRow = currentRow.DataRowState != DataRowState.Unchanged;

                    if (shouldPreserveRow)
                    {
                        // Keep local row intact.
                    }
                    else if (mergeOptions.ExcludeTablesFromRowDeletion.Contains(currentDataTable.TableName))
                    {
                        // Keep local row intact.
                    }
                    else
                    {
                        currentDataTable.RemoveRowAt(i);
                        mergeOptions.DataSetMergeResult.DeletedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, currentRow));
                    }

                    continue;
                }

                bool changed = currentRow.DoRefreshMergePreservingLocalChanges(refreshedRow, currentDataTable.TableName, currentDataTable.Columns, mergeOptions);
                if (changed)
                {
                    mergeOptions.DataSetMergeResult.UpdatedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, currentRow));
                }

                mergedPrimaryKeys.Add(pkValue);
            }
        }

        private static void AddNonProcessedRefreshedRows(
            IDataTable currentDataTable,
            IMergeOptions mergeOptions,
            Dictionary<string, IDataRow> refreshedIndex,
            HashSet<string> mergedPrimaryKeys)
        {
            foreach (KeyValuePair<string, IDataRow> kvp in refreshedIndex)
            {
                string pkValue = kvp.Key;
                if (mergedPrimaryKeys.Contains(pkValue))
                {
                    continue;
                }

                IDataRow newRow = AddNewDataRowWithDefaultValuesToDataTable(currentDataTable);
                IDataRow refreshedRow = kvp.Value;

                newRow.DoRefreshMergePreservingLocalChanges(refreshedRow, currentDataTable.TableName, currentDataTable.Columns, mergeOptions);
                mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, newRow));
            }
        }

        private static IDataRow AddNewDataRowWithDefaultValuesToDataTable(IDataTable dataTable)
        {
            IDataRow newRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(dataTable.Columns);
            dataTable.AddRow(newRow);
            newRow.SetDataRowState(DataRowState.Unchanged);
            return newRow;
        }
    }
}
