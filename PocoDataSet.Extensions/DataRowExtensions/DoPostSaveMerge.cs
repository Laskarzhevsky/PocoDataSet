using System.Collections.Generic;

using PocoDataSet.Extensions.Merging.PostSaveMerge;
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
        /// Does PostSave merge
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="tableName">Table name</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void DoPostSaveMerge(this IDataRow? currentDataRow, IDataRow refreshedDataRow, string tableName, IReadOnlyList<IColumnMetadata> columns, IMergeOptions mergeOptions)
        {
            if (currentDataRow == null)
            {
                return;
            }

            DataRowMerger merger = new DataRowMerger();
            merger.Merge(currentDataRow, refreshedDataRow, columns);
        }
        #endregion
    }
}
