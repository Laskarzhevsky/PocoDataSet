using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.RefreshMergeIfNoChangesExist
{
    /// <summary>
    /// Performs RefreshIfNoChangesExist merge for an observable table.
    /// </summary>
    internal sealed class ObservableDataTableMerger
    {
        public void Merge(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            ObservableDataTableMergeEngine engine = new ObservableDataTableMergeEngine();
            ObservableDataRowMerger rowMerger = new ObservableDataRowMerger();

            engine.MergeKeyed(
                currentObservableDataTable,
                refreshedDataTable,
                observableMergeOptions,
                "RefreshIfNoChangesExist",
                false,
                false,
                true,
                PreserveRowWhenMissingFromRefreshed,
                ValidateAfterPrimaryKeyDiscovery,
                rowMerger);
        }

        static bool PreserveRowWhenMissingFromRefreshed(DataRowState state)
        {
            return state != DataRowState.Unchanged;
        }

        static void ValidateAfterPrimaryKeyDiscovery(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions, int primaryKeyCount)
        {
            
            if (primaryKeyCount <= 0)
            {
                throw new InvalidOperationException(
                    "RefreshIfNoChangesExist requires a primary key for deterministic matching, but table '" + currentObservableDataTable.TableName + "' has no primary key.");
            }

            for (int i = 0; i < currentObservableDataTable.Rows.Count; i++)
            {
                if (currentObservableDataTable.Rows[i].DataRowState != DataRowState.Unchanged)
                {
                    throw new InvalidOperationException(
                        "RefreshIfNoChangesExist cannot be used when table '" + currentObservableDataTable.TableName + "' contains pending changes.");
                }
            }

        }
    }
}
