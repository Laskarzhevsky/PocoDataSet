using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.DoReplaceMerge
{
    /// <summary>
    /// Replace table merge: destructive reload (clear all rows and re-add refreshed rows as Unchanged).
    /// </summary>
    public sealed class DataTableMerger
    {
        #region Public Methods
        /// <summary>
        /// Merges the refreshed data table into the current data table using the "DoReplaceMerge" strategy,
        /// which clears all existing rows in the current data table and adds all rows from the refreshed data table as Unchanged.
        /// This method does not consider any existing data in the current data table and simply replaces it with the refreshed data.
        /// The merge options can be used to track added rows and specify any additional merge behavior.
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        public void Merge(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
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
        #endregion
    }
}
