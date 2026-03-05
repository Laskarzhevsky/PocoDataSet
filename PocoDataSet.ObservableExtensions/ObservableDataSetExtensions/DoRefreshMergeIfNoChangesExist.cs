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
        /// <summary>
        /// Refreshes the observable data set from a refreshed data set, but only when there are no pending local changes in the observable data set
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        public static void DoRefreshMergeIfNoChangesExist(this IObservableDataSet? observableDataSet, IDataSet refreshedDataSet, IObservableMergeOptions observableMergeOptions)
        {
            if (observableDataSet == null)
            {
                return;
            }

            observableMergeOptions.ObservableDataSetMergeResult.Clear();

            ObservableDataSetMerger merger = new ObservableDataSetMerger();
            merger.Merge(observableDataSet, refreshedDataSet, observableMergeOptions);
        }
    }
}
