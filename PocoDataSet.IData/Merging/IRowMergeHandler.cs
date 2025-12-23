using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines row merge handler functionality
    /// </summary>
    public interface IRowMergeHandler
    {
        #region Methods
        /// <summary>
        /// Merges current row with refreshed row
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="currentRow">Current row to update</param>
        /// <param name="refreshedRow">Refreshed row providing values</param>
        /// <param name="columns">Columns collection</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <returns>True if current row values changed</returns>
        bool MergeRow(string tableName, IDataRow currentRow, IDataRow refreshedRow, IList<IColumnMetadata> columns, IMergeOptions mergeOptions);
        #endregion
    }
}
