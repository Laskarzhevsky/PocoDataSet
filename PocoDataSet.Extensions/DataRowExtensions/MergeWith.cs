using System.Collections.Generic;

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
        /// Merges current data row with refreshed data row by copying refreshed values into current data row
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="tableName">Table name</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <returns>True if any value of the current data row changed, otherwise false</returns>
        public static bool MergeWith(this IDataRow currentDataRow, IDataRow refreshedDataRow, string tableName, IList<IColumnMetadata> listOfColumnMetadata, IMergeOptions mergeOptions)
        {
            IRowMergeHandler rowHandler = mergeOptions.GetRowMergeHandler(tableName);
            return rowHandler.MergeRow(tableName, currentDataRow, refreshedDataRow, listOfColumnMetadata, mergeOptions);
        }
        #endregion
    }
}
