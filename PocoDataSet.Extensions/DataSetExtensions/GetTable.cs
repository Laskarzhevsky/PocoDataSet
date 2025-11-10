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
        /// Gets table
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Specified table</returns>
        public static IDataTable? GetTable(this IDataSet dataSet, string tableName)
        {
            if (dataSet.Tables == null)
            {
                return null;
            }

            dataSet.Tables.TryGetValue(tableName, out IDataTable? dataTable);
            return dataTable;
        }
        #endregion
    }
}
