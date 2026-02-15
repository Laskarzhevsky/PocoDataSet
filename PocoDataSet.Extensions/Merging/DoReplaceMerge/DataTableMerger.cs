using System;
using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.DoReplaceMerge
{
    /// <summary>
    /// Replace table merge: destructive reload (clear all rows and re-add refreshed rows as Unchanged).
    /// </summary>
    public sealed class DataTableMerger
    {
        public void Merge(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            if (currentDataTable == null)
            {
                throw new ArgumentNullException(nameof(currentDataTable));
            }

            if (refreshedDataTable == null)
            {
                throw new ArgumentNullException(nameof(refreshedDataTable));
            }

            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            currentDataTable.RemoveAllRows();

            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedRow = refreshedDataTable.Rows[i];

                IDataRow newRow = DataRowFactory.CreateEmpty(refreshedRow.Values.Count);
                newRow.DoReplaceMerge(refreshedRow, currentDataTable.TableName, currentDataTable.Columns, mergeOptions);

                currentDataTable.AddLoadedRow(newRow);
                mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, newRow));
            }
        }
    }
}
