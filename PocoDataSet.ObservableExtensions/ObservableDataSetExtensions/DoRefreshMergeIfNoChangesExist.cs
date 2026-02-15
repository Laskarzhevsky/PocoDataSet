using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableExtensions.Merging.RefreshMergeIfNoChangesExist;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods.
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        public static void DoRefreshMergeIfNoChangesExist(this IObservableDataSet currentObservableDataSet, IDataSet refreshedDataSet, IObservableMergeOptions observableMergeOptions)
        {
            if (currentObservableDataSet == null)
            {
                throw new ArgumentNullException(nameof(currentObservableDataSet));
            }

            if (refreshedDataSet == null)
            {
                throw new ArgumentNullException(nameof(refreshedDataSet));
            }

            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            observableMergeOptions.ObservableDataSetMergeResult.Clear();

            ObservableDataSetMerger merger = new ObservableDataSetMerger();
            merger.Merge(currentObservableDataSet, refreshedDataSet, observableMergeOptions);
        }
    }
}
