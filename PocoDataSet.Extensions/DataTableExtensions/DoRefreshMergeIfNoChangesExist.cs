using PocoDataSet.Extensions.Merging.Modes;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Merges current data table with data from refreshed data table
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void DoRefreshMergeIfNoChangesExist(this IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            RefreshIfNoChangesExistDataTableMerger refreshIfNoChangesExistDataTableMerger = new RefreshIfNoChangesExistDataTableMerger();
            refreshIfNoChangesExistDataTableMerger.MergeRefreshIfNoChangesExist(currentDataTable, refreshedDataTable, mergeOptions);
        }
        #endregion
    }
}
