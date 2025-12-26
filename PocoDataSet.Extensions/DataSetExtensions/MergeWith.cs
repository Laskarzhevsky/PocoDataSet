using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Merges current data set with refreshed data set using a specific merge mode.
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeMode">Merge mode</param>
        /// <returns>Data set merged result</returns>
        public static IDataSetMergeResult MergeWith(this IDataSet? currentDataSet, IDataSet? refreshedDataSet, MergeMode mergeMode)
        {
            IMergeOptions mergeOptions = new MergeOptions();
            mergeOptions.MergeMode = mergeMode;
            return MergeWith(currentDataSet, refreshedDataSet, mergeOptions);
        }

        /// <summary>
        /// Merges current data set with refreshed data set by copying all changes from refreshed data set
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <returns>Data set merged result</returns>
        public static IDataSetMergeResult MergeWith(this IDataSet? currentDataSet, IDataSet? refreshedDataSet, IMergeOptions? mergeOptions = null)
        {
            if (mergeOptions == null)
            {
                mergeOptions = new MergeOptions();
            }
            else
            {
                mergeOptions.DataSetMergeResult.Clear();
            }

            if (currentDataSet == null || refreshedDataSet == null)
            {
                return mergeOptions.DataSetMergeResult;
            }

            IDataSetMergeHandler dataSetMergeHandler = mergeOptions!.GetDataSetMergeHandler(currentDataSet.Name);
            dataSetMergeHandler.Merge(currentDataSet, refreshedDataSet, mergeOptions);

            return mergeOptions.DataSetMergeResult;
        }
        #endregion
    }
}
