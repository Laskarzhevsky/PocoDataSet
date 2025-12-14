using System;

using PocoDataSet.Data;
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
        /// Adds new table from POCO interface to data set
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="interfaceType">Interface type</param>
        /// <returns>New table</returns>
        /// <exception cref="KeyDuplicationException">Exception is thrown if dataset contains a table with specified name already</exception>
        public static IDataTable AddNewTableFromPocoInterface(this IDataSet? dataSet, string tableName, Type interfaceType)
        {
            if (dataSet == null)
            {
                return default!;
            }

            if (dataSet.Tables.ContainsKey(tableName))
            {
                throw new KeyDuplicationException($"DataSet contains table with name {tableName} already");
            }

            IDataTable dataTable = new DataTable();
            dataTable.TableName = tableName;
            dataSet.AddTable(dataTable);
            dataTable.AddColumnsFromInterface(interfaceType);

            return dataTable;
        }
        #endregion
    }
}
