using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.DoReplaceMerge
{
    /// <summary>
    /// Replace row merge: overwrite values from the refreshed row and AcceptChanges.
    /// </summary>
    public sealed class DataRowMerger
    {
        public void Merge(IDataRow currentRow, IDataRow refreshedRow, IReadOnlyList<IColumnMetadata> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                string columnName = columns[i].ColumnName;

                if (!refreshedRow.ContainsKey(columnName))
                {
                    continue;
                }

                currentRow[columnName] = refreshedRow[columnName];
            }

            currentRow.AcceptChanges();
        }
    }
}
