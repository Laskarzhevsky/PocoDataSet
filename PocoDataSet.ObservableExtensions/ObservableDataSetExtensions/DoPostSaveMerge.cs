using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableExtensions.Merging.PostSaveMerge;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods.
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        /// <summary>
        /// Merges a post-save response (refreshed data set returned by the server after applying a changeset) into the current
        /// observable data set. This is the merge mode you typically call right after a successful Save
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        public static void DoPostSaveMerge(this IObservableDataSet? observableDataSet, IDataSet refreshedDataSet, IObservableMergeOptions observableMergeOptions)
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
