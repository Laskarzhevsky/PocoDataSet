using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Merges current data set with refreshed data set by copying all changes from refreshed data set
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        public static DataSetsMergeResult MergeWith(this IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions? mergeOptions = null)
        {
            List<IDataRow> listOfAddedDataRows = new List<IDataRow>();
            List<IDataRow> listOfDeletedDataRows = new List<IDataRow>();
            List<IDataRow> listOfUpdatedDataRows = new List<IDataRow>();
            DataSetsMergeResult dataSetsMergeResult = new DataSetsMergeResult(listOfAddedDataRows, listOfDeletedDataRows, listOfUpdatedDataRows);
            if (currentDataSet == null)
            {
                return dataSetsMergeResult;
            }

            if (refreshedDataSet == null)
            {
                return dataSetsMergeResult;
            }

            if (mergeOptions == null) 
            {
                mergeOptions = new MergeOptions();
            }
            
            foreach (KeyValuePair<string, IDataTable> keyValuePair in refreshedDataSet.Tables)
            {
                IDataTable refreshedDataTable = keyValuePair.Value;
                if (refreshedDataTable == null)
                {
                    continue;
                }

                if (mergeOptions.ExcludeTablesFromMerge.Contains(refreshedDataTable.TableName))
                {
                    continue;
                }

                IDataTable? currentDataTable = null;
                if (currentDataSet.Tables.ContainsKey(refreshedDataTable.TableName))
                {
                    currentDataTable = currentDataSet.GetTable(refreshedDataTable.TableName);
                }
                else
                {
                    // No schema creation in this lite version.
                    continue;
                }

                // 1) Replace all rows in the current table by rows from refreshed table if current table has no primary keys defined
                List<string> currentDataTablePrimaryKeyColumnNames = currentDataTable!.GetPrimaryKeyColumnNames(mergeOptions);
                if (currentDataTablePrimaryKeyColumnNames.Count == 0 && mergeOptions.ReplaceAllRowsInTableWhenTableHasNoPrimaryKeyDefined)
                {
                    currentDataTable!.ReplaceAllRowsByRowsFrom(refreshedDataTable);
                    continue;
                }

                Func<IDataRow, bool>? pruneScope = mergeOptions.GetPruneFilter(currentDataTable!.TableName);
                Dictionary<string, IDataRow> currentDataTableIndex = currentDataTable.BuildDataRowIndex(currentDataTablePrimaryKeyColumnNames, pruneScope);
                HashSet<string> processedPrimaryKeyValues = new HashSet<string>(StringComparer.Ordinal);

                // 2) Upsert pass (respect scope if provided)
                foreach (var refreshedDataRow in refreshedDataTable.Rows)
                {
                    if (pruneScope != null && !pruneScope(refreshedDataRow))
                    {
                        continue;  // ignore out-of-scope refreshed rows
                    }

                    string primaryKeyValue = refreshedDataRow.CompilePrimaryKeyValue(currentDataTablePrimaryKeyColumnNames);
                    if (currentDataTableIndex.TryGetValue(primaryKeyValue, out var currentIndexedRow))
                    {
                        bool rowValueChanged = currentIndexedRow.MergeWith(refreshedDataRow, currentDataTable.Columns);
                        if (rowValueChanged)
                        {
                            listOfUpdatedDataRows.Add(currentIndexedRow);
                        }
                    }
                    else
                    {
                        IDataRow newDataRow = currentDataTable.AddNewRow();
                        newDataRow.MergeWith(refreshedDataRow, currentDataTable.Columns);
                        currentDataTableIndex[primaryKeyValue] = newDataRow;
                        listOfAddedDataRows.Add(newDataRow);
                    }

                    processedPrimaryKeyValues.Add(primaryKeyValue);
                }

                // 3) Prune pass (only for configured tables; respect scope)
                if (mergeOptions.PruneTables.Contains(currentDataTable.TableName))
                {
                    var listOfDataRowIndexesForRemoval = new List<int>();
                    int i = 0;
                    foreach (IDataRow dataRow in currentDataTable.Rows)
                    {
                        if (pruneScope != null && !pruneScope(dataRow))
                        {
                            i++;
                            continue; // do not touch out-of-scope rows
                        }

                        string compiledPrimaryKeyValue = dataRow.CompilePrimaryKeyValue(currentDataTablePrimaryKeyColumnNames);
                        if (!processedPrimaryKeyValues.Contains(compiledPrimaryKeyValue))
                        {
                            listOfDataRowIndexesForRemoval.Add(i);
                            listOfDeletedDataRows.Add(dataRow);
                        }

                        i++;
                    }

                    for (int r = listOfDataRowIndexesForRemoval.Count - 1; r >= 0; r--)
                    {
                        currentDataTable.Rows.RemoveAt(listOfDataRowIndexesForRemoval[r]);
                    }
                }
            }

            return dataSetsMergeResult;
        }
        #endregion
    }
}
