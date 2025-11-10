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
        /// Get value from row
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        public static T? GetValue<T>(this IDataTable dataTable, int rowIndex, string columnName)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable));
            }

            IDataRow dataRow = dataTable.Rows[rowIndex];
            if (dataRow == null)
            {
                return default;
            }

            return dataRow.GetDataFieldValue<T>(columnName);
        }
        #endregion
    }
}
