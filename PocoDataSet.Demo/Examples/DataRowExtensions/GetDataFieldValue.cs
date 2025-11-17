using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Demo.DataSetExtensions
{
    /// <summary>
    /// Provides examples of POCO DataSet functionality
    /// </summary>
    internal static partial class DataRowExtensionExamples
    {
        #region Public Methods
        /// <summary>
        /// DataRowExtensions.GetDataFieldValue method example
        /// Gets data field value
        /// </summary>
        /// <typeparam name="T">Data field value type</typeparam>
        /// <param name="dataRow">Data row</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Data field value</returns>
        public static T? GetDataFieldValue<T>(IDataRow dataRow, string columnName)
        {
            // DataRowExtensions.GetDataFieldValue method call example
            return dataRow.GetDataFieldValue<T>(columnName);
        }
        #endregion
    }
}
