using System.Collections.Generic;

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
        /// Adds new table to data set
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <returns>New table</returns>
        /// <exception cref="KeyDuplicationException">Exception is thrown if dataset contains a table with specified name already</exception>
        public static IDataTable AddNewTable(this IDataSet? dataSet, string tableName)
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

            return dataTable;
        }

        /// <summary>
        /// Adds new table to data set
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>New table</returns>
        /// <exception cref="KeyDuplicationException">Exception is thrown if dataset contains a table with specified name already</exception>
        public static IDataTable AddNewTable(this IDataSet? dataSet, string tableName, List<IColumnMetadata> listOfColumnMetadata)
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
            dataTable.Columns = listOfColumnMetadata;

            // Populate primary keys from column metadata
            if (listOfColumnMetadata != null)
            {
                for (int i = 0; i < listOfColumnMetadata.Count; i++)
                {
                    IColumnMetadata columnMetadata = listOfColumnMetadata[i];
                    if (columnMetadata != null && columnMetadata.IsPrimaryKey)
                    {
                        dataTable.PrimaryKeys.Add(columnMetadata.ColumnName);
                    }
                }
            }

            dataSet.AddTable(dataTable);

            return dataTable;
        }
        #endregion
    }
}
