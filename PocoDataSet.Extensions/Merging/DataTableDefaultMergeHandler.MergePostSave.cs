using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
public partial class DataTableDefaultMergeHandler
{
    internal void MergePostSave(IDataTable currentDataTable, IDataTable changesetDataTable, IMergeOptions mergeOptions)
            {
                List<string> primaryKeyColumnNames = mergeOptions.GetPrimaryKeyColumnNames(currentDataTable);

                Dictionary<string, IDataRow> currentRowsByPrimaryKey = new Dictionary<string, IDataRow>(StringComparer.Ordinal);
                if (primaryKeyColumnNames.Count > 0)
                {
                    for (int i = 0; i < currentDataTable.Rows.Count; i++)
                    {
                        IDataRow currentRow = currentDataTable.Rows[i];
                        string pkValue;
                        bool hasPrimaryKeyValue = RowIdentityResolver.TryGetPrimaryKeyValue(currentRow, primaryKeyColumnNames, out pkValue);
                        if (!hasPrimaryKeyValue)
                        {
                            pkValue = string.Empty;
                        }
                        if (string.IsNullOrEmpty(pkValue))
                        {
                            continue;
                        }
                        if (!currentRowsByPrimaryKey.ContainsKey(pkValue))
                        {
                            currentRowsByPrimaryKey.Add(pkValue, currentRow);
                        }
                    }
                }

                Dictionary<Guid, IDataRow> currentRowsByClientKey = currentDataTable.BuildClientKeyIndex();

                for (int i = 0; i < changesetDataTable.Rows.Count; i++)
                {
                    IDataRow changesetRow = changesetDataTable.Rows[i];

                    if (changesetRow.DataRowState == DataRowState.Added)
                    {
                        currentDataTable.ApplyPostSaveRow( changesetRow, primaryKeyColumnNames, currentRowsByPrimaryKey, currentRowsByClientKey, mergeOptions);
                        continue;
                    }

                    if (changesetRow.DataRowState == DataRowState.Modified)
                    {
                        currentDataTable.ApplyPostSaveRow( changesetRow, primaryKeyColumnNames, currentRowsByPrimaryKey, currentRowsByClientKey, mergeOptions);
                        continue;
                    }

                    if (changesetRow.DataRowState == DataRowState.Deleted)
                    {
                        currentDataTable.ApplyPostSaveDelete( changesetRow, primaryKeyColumnNames, currentRowsByPrimaryKey, currentRowsByClientKey, mergeOptions);
                        continue;
                    }
                }
            }
}
}
