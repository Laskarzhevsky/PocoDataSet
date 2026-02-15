using System;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.Modes
{
    /// <summary>
    /// Replaces the entire observable table with refreshed data (destructive reload).
    /// </summary>
    public sealed class ObservableReplaceDataTableMerger
    {
        public void Replace(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            ObservableDataTableDefaultMergeHandler handler = new ObservableDataTableDefaultMergeHandler();
            handler.MergeObservableDataRowsWithoutPrimaryKeys(currentObservableDataTable, refreshedDataTable, observableMergeOptions);
        }
    }
}
