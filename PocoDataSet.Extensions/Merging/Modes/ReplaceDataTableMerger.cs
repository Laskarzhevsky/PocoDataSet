using System;
using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.Modes
{
    /// <summary>
    /// Replaces the entire local table with refreshed data (destructive reload).
    /// </summary>
    public sealed class ReplaceDataTableMerger
    {
        public void Replace(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            // Destructive reload: clear all rows and re-add refreshed rows as loaded (Unchanged).
            currentDataTable.RemoveAllRows();

            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedRow = refreshedDataTable.Rows[i];

                // Create a new row and copy values for columns that exist in the current table.
                IDataRow newRow = DataRowFactory.CreateEmpty(refreshedRow.Values.Count);
                foreach (var kvp in refreshedRow.Values)
                {
                    if (currentDataTable.ContainsColumn(kvp.Key))
                    {
                        newRow[kvp.Key] = kvp.Value;
                    }
                }

                newRow.AcceptChanges();
                currentDataTable.AddLoadedRow(newRow);
            }
        }
    }
}
