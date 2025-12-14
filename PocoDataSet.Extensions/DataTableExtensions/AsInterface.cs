using System;

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
        /// <exception cref="ArgumentOutOfRangeException">Exception is thrown if table does not have row with specified index</exception>
        public static TInterface? AsInterface<TInterface>(this IDataTable? dataTable, int rowIndex) where TInterface : class
        {
            if (dataTable == null)
            {
                return default(TInterface);
            }

            if (rowIndex < 0 || rowIndex >= dataTable.Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            IDataRow dataRow = dataTable.Rows[rowIndex];
            return DataRowExtensions.AsInterface<TInterface>(dataRow);
        }
        #endregion
    }
}
