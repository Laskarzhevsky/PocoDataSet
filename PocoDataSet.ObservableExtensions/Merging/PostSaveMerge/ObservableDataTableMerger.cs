using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.PostSaveMerge
{
    /// <summary>
    /// Performs PostSave merge for an observable table.
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
                "PostSave",
                true,
                true,
                false,
                PreserveRowWhenMissingFromRefreshed,
                ValidateAfterPrimaryKeyDiscovery,
                rowMerger);
        }

        static bool PreserveRowWhenMissingFromRefreshed(DataRowState state)
        {
            return true;
        }

        static void ValidateAfterPrimaryKeyDiscovery(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions, int primaryKeyCount)
        {
            // No requirements for PK; PostSave can correlate by __ClientKey.

        }
    }
}
