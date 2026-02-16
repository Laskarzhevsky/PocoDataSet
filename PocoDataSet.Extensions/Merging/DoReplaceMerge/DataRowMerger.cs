using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.DoReplaceMerge
{
    /// <summary>
    /// Replace row merge: overwrite values from the refreshed row and AcceptChanges.
    /// </summary>
    public sealed class DataRowMerger
    {
        #region Public Methods
        /// <summary>
        /// Merges the refreshed row into the current row by overwriting all values and calling AcceptChanges.
        /// </summary>
        /// <param name="currentRow">Current row</param>
        /// <param name="refreshedRow">Refreshed row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        public void Merge(IDataRow currentRow, IDataRow refreshedRow, IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
        {
            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                string columnName = listOfColumnMetadata[i].ColumnName;

                if (!refreshedRow.ContainsKey(columnName))
                {
                    continue;
                }

                currentRow[columnName] = refreshedRow[columnName];
            }

            currentRow.AcceptChanges();
        }
        #endregion
    }
}
