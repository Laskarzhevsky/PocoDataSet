using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Gets field value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Field value</returns>
        /// <exception cref="KeyNotFoundException">Exception is thrown if dataset does not contain a table with specified name or column with specified name in that table</exception>
        /// <exception cref="ArgumentOutOfRangeException">Exception is thrown if table does not have row with specified index</exception>
        public static T? GetFieldValue<T>(this IDataSet? dataSet, string tableName, int rowIndex, string columnName)
        {
            if (dataSet == null)
            {
                return default(T);
            }

            IDataTable? dataTable = dataSet.Tables[tableName];
            if (dataTable == null)
            {
                throw new KeyNotFoundException($"DataSet does not contain table with name {tableName}.");
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
