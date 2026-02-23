using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.RefreshMergePreservingLocalChanges
{
    /// <summary>
    /// Performs RefreshPreservingLocalChanges merge for an observable data set.
    /// </summary>
    internal sealed class ObservableDataSetMerger
    {
        public void Merge(IObservableDataSet currentObservableDataSet, IDataSet refreshedDataSet, IObservableMergeOptions observableMergeOptions)
        {
            HashSet<string> mergedTableNames = new HashSet<string>();

            MergeExistingTables(currentObservableDataSet, refreshedDataSet, observableMergeOptions, mergedTableNames);
            AddNonExistingTables(currentObservableDataSet, refreshedDataSet, observableMergeOptions, mergedTableNames);
        }

        static void MergeExistingTables(IObservableDataSet currentObservableDataSet, IDataSet refreshedDataSet, IObservableMergeOptions observableMergeOptions, HashSet<string> mergedTableNames)
        {
            foreach (IObservableDataTable currentTable in currentObservableDataSet.Tables.Values)
            {
                mergedTableNames.Add(currentTable.TableName);

                if (observableMergeOptions.ExcludeTablesFromMerge.Contains(currentTable.TableName))
                {
                    continue;
                }

                IDataTable? refreshedTable;
                refreshedDataSet.TryGetTable(currentTable.TableName, out refreshedTable);

                if (refreshedTable == null)
                {
                    continue;
                }

//                currentTable.DoRefreshMergePreservingLocalChanges(refreshedTable, observableMergeOptions);
                ObservableDataTableMerger merger = new ObservableDataTableMerger();
                merger.Merge(currentTable, refreshedTable, observableMergeOptions);
            }
        }

        static void AddNonExistingTables(IObservableDataSet currentObservableDataSet, IDataSet refreshedDataSet, IObservableMergeOptions observableMergeOptions, HashSet<string> mergedTableNames)
        {
            foreach (IDataTable refreshedTable in refreshedDataSet.Tables.Values)
            {
                if (mergedTableNames.Contains(refreshedTable.TableName))
                {
                    continue;
                }

                if (observableMergeOptions.ExcludeTablesFromMerge.Contains(refreshedTable.TableName))
                {
                    continue;
                }

                IDataTable? cloned = refreshedTable.Clone();
                if (cloned == null)
                {
                    continue;
                }

                IObservableDataTable added = currentObservableDataSet.AddObservableTable(cloned);

                for (int i = 0; i < added.Rows.Count; i++)
                {
                    observableMergeOptions.ObservableDataSetMergeResult.AddedObservableDataRows.Add(
                        new ObservableDataSetMergeResultEntry(cloned.TableName, added.Rows[i]));
                }
            }
        }
    }
}
