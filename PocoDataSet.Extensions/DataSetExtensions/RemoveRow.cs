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
        /// Removes row from table
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        public static void RemoveRow(this IDataSet? dataSet, string tableName, int rowIndex)
        {
            if (dataSet == null)
            {
                return;
            }

            if (!dataSet.Tables.TryGetValue(tableName, out IDataTable? dataTable))
            {
                return;
            }

            if (rowIndex < 0 || rowIndex >= dataTable.Rows.Count)
            {
                return;
            }

            dataTable.Rows.RemoveAt(rowIndex);
        }
        #endregion
    }
}
