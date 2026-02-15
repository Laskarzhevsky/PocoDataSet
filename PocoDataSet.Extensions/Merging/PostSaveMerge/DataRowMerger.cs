using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.PostSaveMerge
{
    /// <summary>
    /// PostSave row merge: copy server-confirmed values into the target row and AcceptChanges.
    /// </summary>
    public sealed class DataRowMerger
    {
        public void Merge(IDataRow targetRow, IDataRow sourceRow, IReadOnlyList<IColumnMetadata> targetColumns)
        {
            for (int c = 0; c < targetColumns.Count; c++)
            {
                string columnName = targetColumns[c].ColumnName;

                if (!sourceRow.ContainsKey(columnName))
                {
                    continue;
                }

                targetRow[columnName] = sourceRow[columnName];
            }

            targetRow.AcceptChanges();
        }
    }
}
