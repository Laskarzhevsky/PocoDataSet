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
        public static void DeleteRowAt(this IDataTable? dataTable, int rowIndex)
        {
            if (dataTable == null)
            {
                return;
            }

            if (rowIndex < 0 || rowIndex >= dataTable.Rows.Count)
            {
                return;
            }

            IDataRow row = dataTable.Rows[rowIndex];
            dataTable.DeleteRow(row);
        }
        #endregion
    }
}
