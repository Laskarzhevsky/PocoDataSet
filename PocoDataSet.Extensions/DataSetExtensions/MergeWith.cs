using System;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Backward-compatible overload that routes to the explicit Do* merge methods.
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeMode">Merge mode</param>
        [Obsolete("This method is deprecated. Use DoPostSaveMerge, DoRefreshMergeIfNoChangesExist, DoRefreshMergePreservingLocalChanges or DoReplaceMerge")]
        public static IDataSetMergeResult MergeWith(this IDataSet? currentDataSet, IDataSet? refreshedDataSet, MergeMode mergeMode)
        {
            IMergeOptions mergeOptions = new MergeOptions();
            mergeOptions.MergeMode = mergeMode;

            return MergeWith(currentDataSet, refreshedDataSet, mergeOptions);
        }

        /// <summary>
        /// Merges current data set with refreshed data set by routing to the new explicit Do* merge methods.
        /// Preserves backward compatibility for existing callers of MergeWith.
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        [Obsolete("This method is deprecated. Use DoPostSaveMerge, DoRefreshMergeIfNoChangesExist, DoRefreshMergePreservingLocalChanges or DoReplaceMerge")]
        public static IDataSetMergeResult MergeWith(this IDataSet? currentDataSet, IDataSet? refreshedDataSet, IMergeOptions? mergeOptions = null)
        {
            if (currentDataSet == null)
            {
                return default!;
            }

            if (refreshedDataSet == null)
            {
                return default!;
            }

            if (mergeOptions == null)
            {
                mergeOptions = new MergeOptions();
                mergeOptions.MergeMode = MergeMode.RefreshPreservingLocalChanges;
            }
            else
            {
                mergeOptions.DataSetMergeResult.Clear();
            }

            MergeMode mergeMode = mergeOptions.MergeMode;
            if (mergeMode == MergeMode.PostSave)
            {
                currentDataSet.DoPostSaveMerge(refreshedDataSet, mergeOptions);
            }
            else if (mergeMode == MergeMode.RefreshIfNoChangesExist)
            {
                currentDataSet.DoRefreshMergeIfNoChangesExist(refreshedDataSet, mergeOptions);
            }
            else if (mergeMode == MergeMode.RefreshPreservingLocalChanges)
            {
                currentDataSet.DoRefreshMergePreservingLocalChanges(refreshedDataSet, mergeOptions);
            }
            else if (mergeMode == MergeMode.Replace)
            {
                currentDataSet.DoReplaceMerge(refreshedDataSet, mergeOptions);
            }
            else
            {
                throw new NotSupportedException("Unsupported merge mode: " + mergeMode);
            }

            return mergeOptions.DataSetMergeResult;
        }
        #endregion
    }
}
