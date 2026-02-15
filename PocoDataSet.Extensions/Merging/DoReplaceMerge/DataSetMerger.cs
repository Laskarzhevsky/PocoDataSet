using System;
using System.Collections.Generic;

using PocoDataSet.Data;
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
            if (currentDataSet == null)
            {
                throw new ArgumentNullException(nameof(currentDataSet));
            }

            if (refreshedDataSet == null)
            {
                throw new ArgumentNullException(nameof(refreshedDataSet));
            }

            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            HashSet<string> mergedTableNames = new HashSet<string>(StringComparer.Ordinal);

            MergeExistingTables(currentDataSet, refreshedDataSet, mergeOptions, mergedTableNames);
            AddNonExistingTables(currentDataSet, refreshedDataSet, mergeOptions, mergedTableNames);
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

                currentTable.DoReplaceMerge(refreshedTable, mergeOptions);
            }
        }

        private static void AddNonExistingTables(IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions, HashSet<string> mergedTableNames)
        {
            foreach (IDataTable table in refreshedDataSet.Tables.Values)
            {
                if (mergedTableNames.Contains(table.TableName))
                {
                    continue;
                }

                if (mergeOptions.ExcludeTablesFromMerge.Contains(table.TableName))
                {
                    continue;
                }

                IDataTable? cloned = table.Clone();
                if (cloned == null)
                {
                    continue;
                }

                currentDataSet.AddTable(cloned);

                for (int i = 0; i < cloned.Rows.Count; i++)
                {
                    mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(cloned.TableName, cloned.Rows[i]));
                }
            }
        }
    }
}
