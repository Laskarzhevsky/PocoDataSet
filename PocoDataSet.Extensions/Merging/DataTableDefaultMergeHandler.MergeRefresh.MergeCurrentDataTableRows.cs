using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
public partial class DataTableDefaultMergeHandler
{
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

                        if (mergeOptions.MergeMode == MergeMode.RefreshPreservingLocalChanges)
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
}
}
