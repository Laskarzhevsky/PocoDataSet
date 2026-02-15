using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.DoReplaceMerge
{
    /// <summary>
    /// Performs Replace merge for an observable table.
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

            engine.MergeObservableDataRowsWithoutPrimaryKeys(currentObservableDataTable, refreshedDataTable, observableMergeOptions, "Replace", rowMerger);
        }
    }
}
