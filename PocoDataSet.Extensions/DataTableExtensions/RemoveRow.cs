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
        /// Removes row from data table
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

            if (!dataTable.ContainsRow(dataRow))
            {
                return;
            }

            dataTable.RemoveRow(dataRow); // always physical
        }
        #endregion
    }
}
