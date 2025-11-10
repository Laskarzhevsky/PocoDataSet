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
        /// Removes table from data set
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="dataTable">Data table for removal</param>
        public static void RemoveTable(this IDataSet dataSet, string tableName)
        {
            dataSet.Tables.Remove(tableName);
        }
        #endregion
    }
}
