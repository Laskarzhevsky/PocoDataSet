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
        /// Removes row from table
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        /// <exception cref="KeyNotFoundException">Exception is thrown if dataset does not contains a table with specified name</exception>
        /// <exception cref="ArgumentOutOfRangeException">Exception is thrown if table does not have row with specified index</exception>
        public static void RemoveRow(this IDataSet? dataSet, string tableName, int rowIndex)
        {
            if (dataSet == null)
            {
                return;
            }

            if (!dataSet.Tables.TryGetValue(tableName, out IDataTable? dataTable))
            {
                throw new KeyNotFoundException($"DataSet does not contain table with name {tableName}.");
            }

            dataTable.RemoveRowAt(rowIndex);
        }

        /// <summary>
        /// Removes row from table using state-aware semantics.
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="dataRow">Data row to remove</param>
        /// <exception cref="KeyNotFoundException">Thrown if dataset does not contain a table with specified name</exception>
        public static void RemoveRow(this IDataSet? dataSet, string tableName, IDataRow? dataRow)
        {
            if (dataSet == null)
            {
                return;
            }

            if (dataRow == null)
            {
                return;
            }

            if (!dataSet.Tables.TryGetValue(tableName, out IDataTable? dataTable))
            {
                throw new KeyNotFoundException($"DataSet does not contain table with name {tableName}.");
            }

            dataTable.RemoveRow(dataRow);
        }
        #endregion
    }
}
