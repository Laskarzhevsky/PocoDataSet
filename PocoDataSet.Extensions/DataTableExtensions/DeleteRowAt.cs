using System;

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
        /// Deletes row from table by row index
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="rowIndex">Row index</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception is thrown if table does not have row with specified index</exception>
        public static void DeleteRowAt(this IDataTable? dataTable, int rowIndex)
        {
            if (dataTable == null)
            {
                return;
            }

            if (rowIndex < 0 || rowIndex >= dataTable.Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            IDataRow row = dataTable.Rows[rowIndex];
            dataTable.DeleteRow(row);
        }
        #endregion
    }
}
