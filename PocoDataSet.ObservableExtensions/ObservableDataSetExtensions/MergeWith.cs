using PocoDataSet.IData;
using PocoDataSet.IObservableData;

using System;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Backward-compatible overload that routes to the explicit Do* merge methods.
        /// </summary>
        /// <param name="currentObservableDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeMode">Merge mode</param>
        [Obsolete("This method is deprecated. Use DoPostSaveMerge, DoRefreshMergeIfNoChangesExist, DoRefreshMergePreservingLocalChanges or DoReplaceMerge")]
        public static IObservableDataSetMergeResult MergeWith(this IObservableDataSet? currentObservableDataSet, IDataSet? refreshedDataSet, MergeMode mergeMode)
        {
            IObservableMergeOptions observableMergeOptions = new ObservableMergeOptions();
            observableMergeOptions.MergeMode = mergeMode;

            return MergeWith(currentObservableDataSet, refreshedDataSet, observableMergeOptions);
        }

        /// <summary>
        /// Merges current data set with refreshed data set by routing to the new explicit Do* merge methods.
        /// Preserves backward compatibility for existing callers of MergeWith.
        /// </summary>
        /// <param name="currentObservableDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="observableMergeOptions">Merge options</param>
        [Obsolete("This method is deprecated. Use DoPostSaveMerge, DoRefreshMergeIfNoChangesExist, DoRefreshMergePreservingLocalChanges or DoReplaceMerge")]
        public static IObservableDataSetMergeResult MergeWith(this IObservableDataSet? currentObservableDataSet, IDataSet? refreshedDataSet, IObservableMergeOptions? observableMergeOptions = null)
        {
            if (currentObservableDataSet == null)
            {
                return default!;
            }

            if (refreshedDataSet == null)
            {
                return default!;
            }

            if (observableMergeOptions == null)
            {
                observableMergeOptions = new ObservableMergeOptions();
                observableMergeOptions.MergeMode = MergeMode.RefreshPreservingLocalChanges;
            }
            else
            {
                observableMergeOptions.ObservableDataSetMergeResult.Clear();
            }

            MergeMode mergeMode = observableMergeOptions.MergeMode;
            if (mergeMode == MergeMode.PostSave)
            {
                currentObservableDataSet.DoPostSaveMerge(refreshedDataSet, observableMergeOptions);
            }
            else if (mergeMode == MergeMode.RefreshIfNoChangesExist)
            {
                currentObservableDataSet.DoRefreshMergeIfNoChangesExist(refreshedDataSet, observableMergeOptions);
            }
            else if (mergeMode == MergeMode.RefreshPreservingLocalChanges)
            {
                currentObservableDataSet.DoRefreshMergePreservingLocalChanges(refreshedDataSet, observableMergeOptions);
            }
            else if (mergeMode == MergeMode.Replace)
            {
                currentObservableDataSet.DoReplaceMerge(refreshedDataSet, observableMergeOptions);
            }
            else
            {
                throw new NotSupportedException("Unsupported merge mode: " + mergeMode);
            }

            return observableMergeOptions.ObservableDataSetMergeResult;
        }
        #endregion
    }
}
