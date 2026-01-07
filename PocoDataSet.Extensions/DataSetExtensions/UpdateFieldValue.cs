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
        /// Updates field value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        /// <param name="fieldValue">Field value</param>
        /// <exception cref="KeyNotFoundException">Exception is thrown if dataset does not contain a table with specified name or column with specified name in that table</exception>
        /// <exception cref="ArgumentOutOfRangeException">Exception is thrown if table does not have row with specified index</exception>
        public static void UpdateFieldValue<T>(this IDataSet? dataSet, string tableName, int rowIndex, string columnName, T? fieldValue)
        {
            if (dataSet == null)
            {
                return;
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

            // Use state-aware update (handles Deleted guard, snapshot, and Modified transition)
            dataRow[columnName] = fieldValue;
        }
        #endregion
    }
}
