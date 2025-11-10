using System;

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
        /// Adds table to data set
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="dataTable">Data table for addition</param>
        public static void AddTable(this IDataSet dataSet, IDataTable dataTable)
        {
            if (dataTable == null || string.IsNullOrEmpty(dataTable.TableName))
            {
                throw new ArgumentException("Table or TableName cannot be null.");
            }

            if (dataSet.Tables.ContainsKey(dataTable.TableName))
            {
                throw new InvalidOperationException($"Table {dataTable.TableName} already exists.");
            }

            dataSet.Tables.Add(dataTable.TableName, dataTable);
        }
        #endregion
    }
}
