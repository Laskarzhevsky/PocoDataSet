using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataSetExtensions
    {
        /// <summary>
        /// Creates data table and populates its rows from POCO items
        /// </summary>
        /// <typeparam name="T">POCO item type</typeparam>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="pocoItems">POCO items</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>Created table</returns>
        public static IDataTable PocoListToDataTable<T>(this IDataSet dataSet, string tableName, IList<T> pocoItems, List<IColumnMetadata> listOfColumnMetadata)
        {
            IDataTable dataTable = dataSet.AddNewTable(tableName, listOfColumnMetadata);
            dataTable.CopyFromPocoList(pocoItems);

            return dataTable;
        }
    }
}
