using System;
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
        /// Gets field value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="dataTable">Data table</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Field value</returns>
        /// <exception cref="KeyNotFoundException">Exception is thrown if table does not contain the column with specified name</exception>
        /// <exception cref="ArgumentOutOfRangeException">Exception is thrown if table does not have row with specified index</exception>
        public static T? GetFieldValue<T>(this IDataTable? dataTable, int rowIndex, string columnName)
        {
            if (dataTable == null)
            {
                return default(T);
            }

            if (rowIndex < 0 || rowIndex >= dataTable.Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            if (!dataTable.ContainsColumn(columnName))
            {
                throw new KeyNotFoundException(nameof(columnName));
            }

            IDataRow dataRow = dataTable.Rows[rowIndex];
            return DataRowExtensions.GetDataFieldValue<T>(dataRow, columnName);
        }
        #endregion
    }
}
