using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Gets "live" data row as an interface
        /// </summary>
        /// <typeparam name="TInterface">POCO interface type</typeparam>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        /// <returns>"Live" data row as an interface</returns>
        public static TInterface? AsInterface<TInterface>(this IDataTable? dataTable, int rowIndex) where TInterface : class
        {
            if (dataTable == null)
            {
                return default(TInterface);
            }

            IDataRow dataRow = dataTable.Rows[rowIndex];
            return DataRowExtensions.AsInterface<TInterface>(dataRow);
        }
        #endregion
    }
}
