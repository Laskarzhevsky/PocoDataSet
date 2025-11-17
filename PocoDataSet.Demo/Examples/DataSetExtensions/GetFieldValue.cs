using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Demo.DataSetExtensions
{
    /// <summary>
    /// Provides examples of POCO DataSet functionality
    /// </summary>
    internal static partial class DataSetExtensionExamples
    {
        #region Public Methods
        /// <summary>
        /// GetFieldValue extension method example
        /// Gets field value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Field value</returns>
        public static T? GetFieldValue<T>(IDataSet dataSet, string tableName, int rowIndex, string columnName)
        {
            // GetFieldValue extension method call example
            return dataSet.GetFieldValue<T>(tableName, rowIndex, columnName);
        }
        #endregion
    }
}
