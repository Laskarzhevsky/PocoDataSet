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
        /// RemoveTable extension method example
        /// Removes table from data set
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="dataTable">Data table for removal</param>
        public static void RemoveTable(IDataSet dataSet, string tableName)
        {
            // RemoveTable extension method call example
            dataSet.RemoveTable(tableName);
        }
        #endregion
    }
}
