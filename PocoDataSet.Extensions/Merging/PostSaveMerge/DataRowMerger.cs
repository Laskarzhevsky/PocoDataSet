using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.PostSaveMerge
{
    /// <summary>
    /// PostSave row merge: copy server-confirmed values into the target row and AcceptChanges.
    /// </summary>
    public sealed class DataRowMerger
    {
        #region Public Methods
        /// <summary>
        /// Merges the refreshed row into the current row by copying only different values and
        /// calling AcceptChanges if any value was changed or the row is not Unchanged.
        /// </summary>
        /// <param name="currentRow">Current row</param>
        /// <param name="refreshedRow">Refreshed row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        public void Merge(IDataRow currentRow, IDataRow refreshedRow, IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
        {
            bool anyValueChanged = false;

            for (int c = 0; c < listOfColumnMetadata.Count; c++)
            {
                string columnName = listOfColumnMetadata[c].ColumnName;

                if (!refreshedRow.ContainsKey(columnName))
                {
                    continue;
                }

                object? oldValue;
                currentRow.TryGetValue(columnName, out oldValue);

                object? newValue;
                refreshedRow.TryGetValue(columnName, out newValue);

                if (DataFieldValuesComparer.FieldValuesEqual(oldValue, newValue))
                {
                    continue;
                }

                currentRow[columnName] = newValue;
                anyValueChanged = true;
            }

            // AcceptChanges is required to complete PostSave semantics (Modified/Added -> Unchanged).
            // But if the row is already Unchanged and values are identical, calling AcceptChanges would be a no-op.
            // The critical part is to avoid assigning identical values (which would capture OriginalValues churn).
            if (anyValueChanged || currentRow.DataRowState != DataRowState.Unchanged)
            {
                currentRow.AcceptChanges();
            }
        }
        #endregion
    }
}
