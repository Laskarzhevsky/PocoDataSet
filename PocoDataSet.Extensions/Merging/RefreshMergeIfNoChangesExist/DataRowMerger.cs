using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.RefreshMergeIfNoChangesExist
{
    /// <summary>
    /// Refresh-if-clean row merge: current dataset is expected to be clean; overwrite values and AcceptChanges.
    /// </summary>
    public sealed class DataRowMerger
    {
        public bool Merge(IDataRow currentRow, IDataRow refreshedRow, IReadOnlyList<IColumnMetadata> columns)
        {
            bool changed = false;

            for (int i = 0; i < columns.Count; i++)
            {
                string columnName = columns[i].ColumnName;

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
