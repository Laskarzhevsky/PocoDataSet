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
        /// Removes table from data set
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="dataTable">Data table for removal</param>
        /// <exception cref="KeyNotFoundException">Exception is thrown if dataset does not contains a table with specified name</exception>
        public static void RemoveTable(this IDataSet? dataSet, string tableName)
        {
            if (dataSet is null)
            {
                return;
            }

            if (dataSet.Tables.ContainsKey(tableName))
            {
                dataSet.Tables.Remove(tableName);
            }

            throw new KeyNotFoundException($"DataSet does not contain table with name {tableName}.");
        }
        #endregion
    }
}
