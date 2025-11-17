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
        /// <returns>new table</returns>
        public static IDataTable AddNewTable(this IDataSet? dataSet, string tableName)
        {
            if (dataSet == null)
            {
                return default!;
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
        public static IDataTable AddNewTable(this IDataSet? dataSet, string tableName, List<IColumnMetadata> listOfColumnMetadata)
        {
            if (dataSet == null)
            {
                return default!;
            }

            IDataTable dataTable = new DataTable();
            dataTable.TableName = tableName;
            dataTable.Columns = listOfColumnMetadata;
            dataSet.AddTable(dataTable);

            return dataTable;
        }
        #endregion
    }
}
