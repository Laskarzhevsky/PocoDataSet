using System.Collections.Generic;
using PocoDataSet.IData;

namespace PocoDataSet.IDataMerge
{
    /// <summary>
    /// Defines row merge handler.
    /// </summary>
    public interface IRowMergeHandler
    {
        /// <summary>
        /// Merges current row with refreshed row.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="currentRow">Current row to update.</param>
        /// <param name="refreshedRow">Refreshed row providing values.</param>
        /// <param name="columns">Columns collection.</param>
        /// <param name="context">Merge context.</param>
        /// <returns>True if current row values changed.</returns>
        bool MergeRow(string tableName, IDataRow currentRow, IDataRow refreshedRow, IList<IColumnMetadata> columns, IMergeContext context);
    }
}
