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
        /// Clears specified table by removing all rows
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <exception cref="KeyNotFoundException">Exception is thrown if dataset does not contains a table with specified name</exception>
        public static void ClearTable(this IDataSet? dataSet, string tableName)
        {
            if (dataSet == null)
            {
                return;
            }

            if (!dataSet.Tables.TryGetValue(tableName, out IDataTable? dataTable))
            {
                throw new KeyNotFoundException($"DataSet does not contain table with name {tableName}.");
            }

            dataTable.Rows.Clear();
        }
        #endregion
    }
}
