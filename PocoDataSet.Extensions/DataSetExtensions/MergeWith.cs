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
        /// Merges current data set with refreshed data set by copying all changes from refreshed data set
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        public static IDataSetMergeResult MergeWith(this IDataSet? currentDataSet, IDataSet? refreshedDataSet, IMergeOptions? mergeOptions = null)
        {
            if (mergeOptions == null)
            {
                mergeOptions = new MergeOptions();
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
