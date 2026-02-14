using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
public partial class DataTableDefaultMergeHandler
{
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
}
}
