using System.Collections.Generic;

using PocoDataSet.Extensions.Merging.DoReplaceMerge;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data row extension methods
    /// </summary>
    public static partial class DataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Does Replace merge
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="tableName">Table name</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void DoReplaceMerge(this IDataRow? currentDataRow, IDataRow refreshedDataRow, string tableName, IReadOnlyList<IColumnMetadata> listOfColumnMetadata, IMergeOptions mergeOptions)
        {
            if (currentDataRow == null)
            {
                return;
            }

            DataRowMerger merger = new DataRowMerger();
            merger.Merge(currentDataRow, refreshedDataRow, listOfColumnMetadata);
        }
        #endregion
    }
}
