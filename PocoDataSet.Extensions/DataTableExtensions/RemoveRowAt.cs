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
        /// Removes row at specified position from data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="rowIndex">Row index</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception is thrown if table does not have row with specified index</exception>
        public static void RemoveRowAt(this IDataTable? dataTable, int rowIndex)
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
        #endregion
    }
}
