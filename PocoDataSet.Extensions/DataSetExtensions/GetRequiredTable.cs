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
        /// Gets table creating it if not exists
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Specified table</returns>
        public static IDataTable GetRequiredTable(this IDataSet? dataSet, string tableName)
        {
            if (dataSet == null)
            {
                return default!;
            }

            dataSet.Tables.TryGetValue(tableName, out IDataTable? dataTable);
            if (dataTable == null)
            {
                dataTable = dataSet.AddNewTable(tableName);
            }

            return dataTable;
        }
        #endregion
    }
}
