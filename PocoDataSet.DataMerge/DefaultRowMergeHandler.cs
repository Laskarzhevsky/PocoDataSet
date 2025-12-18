using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IDataMerge;

namespace PocoDataSet.DataMerge
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
        /// <param name="mergeContext">Merge context</param>
        /// <returns>True if any value of row sas changed, otherwise false</returns>
        public bool MergeRow(string tableName, IDataRow currentRow, IDataRow refreshedRow, IList<IColumnMetadata> columns, IMergeContext mergeContext)
        {
            return currentRow.MergeWith(refreshedRow, columns);
        }
        #endregion
    }
}
