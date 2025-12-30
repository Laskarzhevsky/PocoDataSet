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
        /// Adds row to table
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="dataRow">Data row for addition</param>
        /// <exception cref="KeyNotFoundException">Exception is thrown if dataset does not contains a table with specified name</exception>
        public static void AddRow(this IDataSet? dataSet, string tableName, IDataRow dataRow)
        {
            if (dataSet == null)
            {
                return;
            }

            if (!dataSet.Tables.TryGetValue(tableName, out IDataTable? dataTable))
            {
                throw new KeyNotFoundException($"DataSet does not contain table with name {tableName}.");
            }

            // If a row is being inserted into a table for the first time, treat it as Added
            if (dataRow.DataRowState == DataRowState.Detached)
            {
                dataRow.SetDataRowState(DataRowState.Added);
            }

            dataTable.AddRow(dataRow);
        }
        #endregion
    }
}
