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
        /// Clears specified table by removing all rows
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        public static void ClearTable(this IDataSet? dataSet, string tableName)
        {
            if (dataSet == null)
            {
                return;
            }

            if (dataSet.Tables.TryGetValue(tableName, out IDataTable? dataTable))
            {
                dataTable.Rows.Clear();
            }
        }
        #endregion
    }
}
