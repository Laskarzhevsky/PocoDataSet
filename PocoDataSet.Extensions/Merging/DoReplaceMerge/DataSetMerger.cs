using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.DoReplaceMerge
{
    /// <summary>
    /// Replace dataset merge: destructive reload per table.
    /// </summary>
    public sealed class DataSetMerger
    {
        public void Merge(IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions)
        {
            // IMPORTANT: ensure MergeResult reflects only this merge run.
            // This protects callers who reuse the same MergeOptions instance across multiple Replace merges,
            // even if they call the lower-level merger directly instead of the extension method.
            mergeOptions.DataSetMergeResult.Clear();

            HashSet<string> mergedTableNames = new HashSet<string>(StringComparer.Ordinal);

            MergeExistingTables(currentDataSet, refreshedDataSet, mergeOptions, mergedTableNames);
            currentDataSet.AddNonExistingTables(refreshedDataSet, mergeOptions, mergedTableNames);
        }

        private static void MergeExistingTables(IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions, HashSet<string> mergedTableNames)
        {
            foreach (IDataTable currentTable in currentDataSet.Tables.Values)
            {
                if (mergeOptions.ExcludeTablesFromMerge.Contains(currentTable.TableName))
                {
                    continue;
                }

                mergedTableNames.Add(currentTable.TableName);

                IDataTable? refreshedTable;
                refreshedDataSet.TryGetTable(currentTable.TableName, out refreshedTable);
                if (refreshedTable == null)
                {
                    continue;
                }

//                currentTable.DoReplaceMerge(refreshedTable, mergeOptions);
                DataTableMerger merger = new DataTableMerger();
                merger.Merge(currentTable, refreshedTable, mergeOptions);
            }
        }
    }
}