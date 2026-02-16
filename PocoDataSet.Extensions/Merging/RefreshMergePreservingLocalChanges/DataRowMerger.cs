using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.RefreshMergePreservingLocalChanges
{
    /// <summary>
    /// Refresh-preserving row merge: copies refreshed values into the current row only when it is safe
    /// (never overwrites Added/Modified/Deleted local work).
    /// </summary>
    public sealed class DataRowMerger
    {
        #region Public Methods
        /// <summary>
        /// Merges the refreshed row into the current row, but only if the current row is not in Added/Modified/Deleted state.
        /// </summary>
        /// <param name="currentRow">Current row</param>
        /// <param name="refreshedRow">Refreshed row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>True if changes were merged, otherwise false</returns>
        public bool Merge(IDataRow currentRow, IDataRow refreshedRow, IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
        {
            // Never overwrite user work.
            if (currentRow.DataRowState == DataRowState.Added ||
                currentRow.DataRowState == DataRowState.Modified ||
                currentRow.DataRowState == DataRowState.Deleted)
            {
                return false;
            }

            bool changed = false;

            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                string columnName = listOfColumnMetadata[i].ColumnName;

                // Be tolerant to schema evolution / special columns.
                object? oldValue;
                currentRow.TryGetValue(columnName, out oldValue);

                object? newValue;
                refreshedRow.TryGetValue(columnName, out newValue);

                if (DataFieldValuesComparer.FieldValuesEqual(oldValue, newValue))
                {
                    continue;
                }

                currentRow[columnName] = newValue;
                changed = true;
            }

            if (changed)
            {
                currentRow.AcceptChanges();
            }

            return changed;
        }
        #endregion
    }
}
