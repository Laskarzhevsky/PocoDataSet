using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.RefreshMergeIfNoChangesExist
{
    /// <summary>
    /// Refresh-if-clean row merge: current dataset is expected to be clean; overwrite values and AcceptChanges.
    /// </summary>
    public sealed class DataRowMerger
    {
        #region Public Methods
        /// <summary>
        /// Merges the refreshed row into the current row if any changes exist.
        /// </summary>
        /// <param name="currentRow">Current row</param>
        /// <param name="refreshedRow">Refreshed row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>True if changes were merged, otherwise false</returns>
        public bool Merge(IDataRow currentRow, IDataRow refreshedRow, IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
        {
            bool changed = false;

            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                string columnName = listOfColumnMetadata[i].ColumnName;

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
