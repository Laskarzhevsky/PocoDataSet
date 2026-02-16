using PocoDataSet.Extensions.Merging.DoReplaceMerge;
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
        /// Dose Replace merge
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void DoReplaceMerge(this IDataSet? currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions)
        {
            if (currentDataSet == null)
            {
                return;
            }

            DataSetMerger merger = new DataSetMerger();
            merger.Merge(currentDataSet, refreshedDataSet, mergeOptions);
        }
        #endregion
    }
}
