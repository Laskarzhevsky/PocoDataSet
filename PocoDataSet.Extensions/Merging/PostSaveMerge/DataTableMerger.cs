using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.PostSaveMerge
{
    /// <summary>
    /// PostSave table merge: applies a server-confirmed changeset back onto the current table.
    /// </summary>
    public sealed class DataTableMerger
    {

        public void Merge(IDataTable currentDataTable, IDataTable changesetDataTable, IMergeOptions mergeOptions)
        {
            if (currentDataTable == null)
            {
                throw new ArgumentNullException(nameof(currentDataTable));
            }

            if (changesetDataTable == null)
            {
                throw new ArgumentNullException(nameof(changesetDataTable));
            }

            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            List<string> primaryKeyColumnNames = mergeOptions.GetPrimaryKeyColumnNames(currentDataTable);

            Dictionary<string, IDataRow> currentRowsByPrimaryKey = BuildPrimaryKeyIndex(currentDataTable, primaryKeyColumnNames);
            Dictionary<Guid, IDataRow> currentRowsByClientKey = RowIdentityResolver.BuildClientKeyIndex(currentDataTable.Rows);

            for (int i = 0; i < changesetDataTable.Rows.Count; i++)
            {
                IDataRow changesetRow = changesetDataTable.Rows[i];

                if (changesetRow.DataRowState == DataRowState.Added || changesetRow.DataRowState == DataRowState.Modified)
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

        private static Dictionary<string, IDataRow> BuildPrimaryKeyIndex(IDataTable dataTable, List<string> primaryKeyColumnNames)
        {
            Dictionary<string, IDataRow> index = new Dictionary<string, IDataRow>(StringComparer.Ordinal);

            if (primaryKeyColumnNames.Count == 0)
            {
                return index;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                IDataRow row = dataTable.Rows[i];

                string pkValue;
                bool ok = RowIdentityResolver.TryGetPrimaryKeyValue(row, primaryKeyColumnNames, out pkValue);
                if (!ok)
                {
                    pkValue = string.Empty;
                }

                if (string.IsNullOrEmpty(pkValue))
                {
                    continue;
                }

                if (!index.ContainsKey(pkValue))
                {
                    index.Add(pkValue, row);
                }
            }

            return index;
        }

        private void ApplyPostSaveRow(
            IDataTable currentDataTable,
            IDataRow changesetRow,
            List<string> primaryKeyColumnNames,
            Dictionary<string, IDataRow> currentRowsByPrimaryKey,
            Dictionary<Guid, IDataRow> currentRowsByClientKey,
            IMergeOptions mergeOptions)
        {
            IDataRow? targetRow = FindTargetRow(changesetRow, primaryKeyColumnNames, currentRowsByPrimaryKey, currentRowsByClientKey);

            bool isNewRowAddedToCurrent = false;
            if (targetRow == null)
            {
                targetRow = AddNewDataRowWithDefaultValuesToDataTable(currentDataTable);
                isNewRowAddedToCurrent = true;
            }

            // Delegate row value application to the row merger via extension chain.
            targetRow.DoPostSaveMerge(changesetRow, currentDataTable.TableName, currentDataTable.Columns, mergeOptions);

            if (isNewRowAddedToCurrent)
            {
                mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, targetRow));
            }
            else
            {
                mergeOptions.DataSetMergeResult.UpdatedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, targetRow));
            }
        }

        private void ApplyPostSaveDelete(
            IDataTable currentDataTable,
            IDataRow changesetRow,
            List<string> primaryKeyColumnNames,
            Dictionary<string, IDataRow> currentRowsByPrimaryKey,
            Dictionary<Guid, IDataRow> currentRowsByClientKey,
            IMergeOptions mergeOptions)
        {
            IDataRow? targetRow = FindTargetRow(changesetRow, primaryKeyColumnNames, currentRowsByPrimaryKey, currentRowsByClientKey);
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

        private static IDataRow? FindTargetRow(
            IDataRow changesetRow,
            List<string> primaryKeyColumnNames,
            Dictionary<string, IDataRow> currentRowsByPrimaryKey,
            Dictionary<Guid, IDataRow> currentRowsByClientKey)
        {
            IDataRow? targetRow = null;

            if (primaryKeyColumnNames.Count > 0)
            {
                string pkValue;
                bool ok = RowIdentityResolver.TryGetPrimaryKeyValue(changesetRow, primaryKeyColumnNames, out pkValue);
                if (!ok)
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
                bool hasClientKey = RowIdentityResolver.TryGetClientKey(changesetRow, out clientKey);
                if (hasClientKey)
                {
                    currentRowsByClientKey.TryGetValue(clientKey, out targetRow);
                }
            }

            return targetRow;
        }

        private static IDataRow AddNewDataRowWithDefaultValuesToDataTable(IDataTable dataTable)
        {
            IDataRow newDataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(dataTable.Columns);
            dataTable.AddRow(newDataRow);
            newDataRow.SetDataRowState(DataRowState.Unchanged);
            return newDataRow;
        }
    }
}
