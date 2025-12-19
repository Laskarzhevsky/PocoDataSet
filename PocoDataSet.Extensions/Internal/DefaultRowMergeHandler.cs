using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Default row merge handler - copies changed field values from refreshed row into current row.
    /// </summary>
    public class DefaultRowMergeHandler : IRowMergeHandler
    {
        #region Public Methods
        /// <summary>
        /// Merges current data row with refreshed data row by copying refreshed values into current data row
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="mergeOptions">Merge options.</param>
        /// <returns>True if any value of row sas changed, otherwise false</returns>
        public bool MergeRow(string tableName, IDataRow currentRow, IDataRow refreshedRow, IList<IColumnMetadata> columns, IMergeOptions mergeOptions)
        {
            return currentRow.MergeWith(refreshedRow, columns);
        }
        #endregion
    }
}
