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
        public bool Merge(IDataRow currentRow, IDataRow refreshedRow, IReadOnlyList<IColumnMetadata> columns)
        {
            // Never overwrite user work.
            if (currentRow.DataRowState == DataRowState.Added ||
                currentRow.DataRowState == DataRowState.Modified ||
                currentRow.DataRowState == DataRowState.Deleted)
            {
                return false;
            }

            bool changed = false;

            for (int i = 0; i < columns.Count; i++)
            {
                string columnName = columns[i].ColumnName;

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
    }
}
