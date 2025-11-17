using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Demo.DataSetExtensions
{
    /// <summary>
    /// Provides examples of POCO DataTable functionality
    /// </summary>
    internal static partial class DataTableExtensionExamples
    {
        #region Public Methods
        /// <summary>
        /// GetFieldValue extension method example
        /// Gets field value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="dataTable">Data table</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Field value</returns>
        public static T? GetFieldValue<T>(IDataTable dataTable, int rowIndex, string columnName)
        {
            // GetFieldValue extension method call example
            return dataTable.GetFieldValue<T>(rowIndex, columnName);
        }
        #endregion
    }
}
