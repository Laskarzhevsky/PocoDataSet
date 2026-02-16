using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.PostSaveMerge
{
    /// <summary>
    /// PostSave dataset merge: applies server-confirmed changesets back onto the current dataset.
    /// </summary>
    public sealed class DataSetMerger
    {
        public void Merge(IDataSet currentDataSet, IDataSet changesetDataSet, IMergeOptions mergeOptions)
        {
            HashSet<string> mergedTableNames = new HashSet<string>(StringComparer.Ordinal);

            MergeExistingTables(currentDataSet, changesetDataSet, mergeOptions, mergedTableNames);
            currentDataSet.AddNonExistingTables(changesetDataSet, mergeOptions, mergedTableNames);
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

                currentTable.DoPostSaveMerge(refreshedTable, mergeOptions);
            }
        }
    }
}
