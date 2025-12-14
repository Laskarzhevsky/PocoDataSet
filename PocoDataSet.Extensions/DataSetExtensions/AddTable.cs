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
        /// <exception cref="ArgumentException">Exception is thrown if specified table does not have assigned name</exception>
        /// <exception cref="KeyDuplicationException">Exception is thrown if a dataset contains a table with specified name already</exception>
        public static void AddTable(this IDataSet? dataSet, IDataTable dataTable)
        {
            if (dataSet == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(dataTable.TableName))
            {
                throw new ArgumentException("Table name cannot be null");
            }

            if (dataSet.Tables.ContainsKey(dataTable.TableName))
            {
                throw new KeyDuplicationException($"DataSet contains table with name {dataTable.TableName} already");
            }

            dataSet.Tables.Add(dataTable.TableName, dataTable);
        }
        #endregion
    }
}
