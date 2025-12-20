using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Replaces all rows by rows from
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void ReplaceAllRowsByRowsFrom(this IDataTable? currentDataTable, IDataTable? refreshedDataTable, IMergeOptions mergeOptions)
        {
            if (currentDataTable == null || refreshedDataTable == null)
            {
                return;
            }

            // Track deletions BEFORE clearing the table
            List<IDataRow> oldRows = new List<IDataRow>(currentDataTable.Rows);

            // Clear current rows
            currentDataTable.Rows.Clear();

            foreach (IDataRow refreshedDataRow in refreshedDataTable.Rows)
            {
                IDataRow newDataRow = currentDataTable.AddNewRow();
                DataRowExtensions.MergeWith(newDataRow, refreshedDataRow, currentDataTable.TableName, currentDataTable.Columns, mergeOptions);

                mergeOptions.DataSetMergeResult.ListOfAddedDataRows.Add(newDataRow);
                for (int i = 0; i < oldRows.Count; i++)
                {
                    mergeOptions.DataSetMergeResult.ListOfDeletedDataRows.Add(oldRows[i]);
                }
            }
        }
        #endregion
    }
}
