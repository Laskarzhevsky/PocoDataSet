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
        /// ClearTable extension method example
        /// Clears specified table by removing all rows
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        public static void ClearTable(IDataSet dataSet, string tableName)
        {
            // ClearTable extension method call example
            dataSet.ClearTable(tableName);
        }
        #endregion
    }
}
