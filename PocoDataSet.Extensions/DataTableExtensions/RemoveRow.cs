using System;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        /// <summary>
        /// Removes row from data table using state-aware semantics.
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="rowIndex">Row index</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception is thrown if table does not have row with specified index</exception>
        public static void RemoveRow(this IDataTable? dataTable, int rowIndex)
        {
            if (dataTable == null)
            {
                return;
            }

            if (rowIndex < 0 || rowIndex >= dataTable.Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            IDataRow dataRow = dataTable.Rows[rowIndex];
            RemoveRow(dataTable, dataRow);
        }

        /// <summary>
        /// Removes row from data table using state-aware semantics.
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="dataRow">Data row to remove</param>
        public static void RemoveRow(this IDataTable? dataTable, IDataRow? dataRow)
        {
            if (dataTable == null)
            {
                return;
            }

            if (dataRow == null)
            {
                return;
            }

            if (!dataTable.Rows.Contains(dataRow))
            {
                return;
            }

            // Case 1: New row → undo creation
            if (dataRow.DataRowState == DataRowState.Added)
            {
                dataTable.Rows.Remove(dataRow);
                dataRow.DataRowState = DataRowState.Detached;
                return;
            }

            // Case 2: Existing row → soft delete
            if (dataRow.DataRowState == DataRowState.Unchanged ||
                dataRow.DataRowState == DataRowState.Modified)
            {
                dataRow.Delete();
                return;
            }

            // Case 3: Deleted / Detached → nothing to do
        }
    }
}
