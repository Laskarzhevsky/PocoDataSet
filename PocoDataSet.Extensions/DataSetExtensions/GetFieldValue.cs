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
        /// Gets field value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Field value</returns>
        public static T? GetFieldValue<T>(this IDataSet? dataSet, string tableName, int rowIndex, string columnName)
        {
            if (dataSet == null)
            {
                return default(T);
            }

            IDataTable? dataTable = dataSet.GetTable(tableName);
            if (dataTable == null)
            {
                return default(T);
            }

            IDataRow dataRow = dataTable.Rows[rowIndex];
            return DataRowExtensions.GetDataFieldValue<T>(dataRow, columnName);
        }
        #endregion
    }
}
