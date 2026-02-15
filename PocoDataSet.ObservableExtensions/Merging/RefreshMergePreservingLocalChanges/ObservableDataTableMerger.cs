using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.RefreshMergePreservingLocalChanges
{
    /// <summary>
    /// Performs RefreshPreservingLocalChanges merge for an observable table.
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
                "RefreshPreservingLocalChanges",
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
                    "RefreshPreservingLocalChanges requires a primary key on table '" + currentObservableDataTable.TableName + "'.");
            }

        }
    }
}
