using PocoDataSet.Extensions.Merging.DoReplaceMerge;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Does Replace merge
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void DoReplaceMerge(this IDataTable? currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            if (currentDataTable == null)
            {
                return;
            }

            DataTableMerger merger = new DataTableMerger();
            merger.Merge(currentDataTable, refreshedDataTable, mergeOptions);
        }
        #endregion
    }
}
