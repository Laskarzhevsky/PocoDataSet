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
        /// Add row to table
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="dataRow">Data row for addition</param>
        public static void AddRow(this IDataSet dataSet, string tableName, IDataRow dataRow)
        {
            if (!dataSet.Tables.TryGetValue(tableName, out IDataTable? dataTable))
            {
                throw new KeyNotFoundException($"Table {tableName} not found.");
            }

            dataTable.Rows.Add(dataRow);
        }
        #endregion
    }
}
