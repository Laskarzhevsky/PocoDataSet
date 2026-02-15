using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
internal partial class DataTableDefaultMergeHandler
{
    internal void MergeRefresh(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
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
                    Dictionary<string, IDataRow> refreshedDataTableDataRowIndex = refreshedDataTable.BuildPrimaryKeyIndex(currentDataTablePrimaryKeyColumnNames);
                    HashSet<string> primaryKeysOfMergedDataRows = new HashSet<string>();

                    MergeCurrentDataTableRows(currentDataTable, refreshedDataTable, mergeOptions, currentDataTablePrimaryKeyColumnNames, refreshedDataTableDataRowIndex, primaryKeysOfMergedDataRows);
                    MergeNonProcessedDataRowsFromRefreshedDataTable(currentDataTable, refreshedDataTable, mergeOptions, refreshedDataTableDataRowIndex, primaryKeysOfMergedDataRows);
                }
            }
}
}
