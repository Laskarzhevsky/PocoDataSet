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
        /// Gets field value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="dataTable">Data table</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Field value</returns>
        public static T? GetFieldValue<T>(this IDataTable? dataTable, int rowIndex, string columnName)
        {
            if (dataTable == null)
            {
                return default(T);
            }

            IDataRow dataRow = dataTable.Rows[rowIndex];
            return DataRowExtensions.GetDataFieldValue<T>(dataRow, columnName);
        }
        #endregion
    }
}
