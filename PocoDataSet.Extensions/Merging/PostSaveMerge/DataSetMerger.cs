using System;
using System.Collections.Generic;

using PocoDataSet.Data;
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
            if (currentDataSet == null)
            {
                throw new ArgumentNullException(nameof(currentDataSet));
            }

            if (changesetDataSet == null)
            {
                throw new ArgumentNullException(nameof(changesetDataSet));
            }

            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            HashSet<string> mergedTableNames = new HashSet<string>(StringComparer.Ordinal);

            MergeExistingTables(currentDataSet, changesetDataSet, mergeOptions, mergedTableNames);
            AddNonExistingTables(currentDataSet, changesetDataSet, mergeOptions, mergedTableNames);
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

        private static void AddNonExistingTables(IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions, HashSet<string> mergedTableNames)
        {
            foreach (IDataTable dataTable in refreshedDataSet.Tables.Values)
            {
                if (mergedTableNames.Contains(dataTable.TableName))
                {
                    continue;
                }

                if (mergeOptions.ExcludeTablesFromMerge.Contains(dataTable.TableName))
                {
                    continue;
                }

                IDataTable? cloned = dataTable.Clone();
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
