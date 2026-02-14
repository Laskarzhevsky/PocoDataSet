using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal static class PostSaveMergeExtensions
    {
        internal static Dictionary<Guid, IDataRow> BuildClientKeyIndex(this IDataTable dataTable)
        {
                        Dictionary<Guid, IDataRow> index = new Dictionary<Guid, IDataRow>();
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            IDataRow row = dataTable.Rows[i];
                            Guid clientKey;
                            if (row.TryGetClientKey(out clientKey))
                            {
                                if (!index.ContainsKey(clientKey))
                                {
                                    index.Add(clientKey, row);
                                }
                            }
                        }
                        return index;

        }

        internal static bool TryGetClientKey(this IDataRow row, out Guid clientKey)
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

        internal static void CopyAllValues(this IDataRow targetRow, IDataRow sourceRow, IReadOnlyList<IColumnMetadata> targetColumns)
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

        internal static void ApplyPostSaveRow(
            this IDataTable currentDataTable,
            IDataRow changesetRow,
            List<string> primaryKeyColumnNames,
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
                            if (changesetRow.TryGetClientKey(out clientKey))
                            {
                                currentRowsByClientKey.TryGetValue(clientKey, out targetRow);
                            }
                        }

                        bool isNewRowAddedToCurrent = false;
                        if (targetRow == null)
                        {
                            // Current table did not contain the row; add it.
                            IDataRow newRow = AddNewDataRowWithDefaultValuesToDataTable(currentDataTable);
                            targetRow = newRow;
                            isNewRowAddedToCurrent = true;
                        }

                        targetRow.CopyAllValues(changesetRow, currentDataTable.Columns);

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

        internal static void ApplyPostSaveDelete(
            this IDataTable currentDataTable,
            IDataRow changesetRow,
            List<string> primaryKeyColumnNames,
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
                            if (changesetRow.TryGetClientKey(out clientKey))
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

private static IDataRow AddNewDataRowWithDefaultValuesToDataTable(IDataTable dataTable)
{
    IDataRow newDataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(dataTable.Columns);
    dataTable.AddRow(newDataRow);
    newDataRow.SetDataRowState(DataRowState.Unchanged);
    return newDataRow;
}
    }
}
