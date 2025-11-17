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
        /// GetRequiredTable extension method example
        /// Gets table creating it if not exists
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Requested table</returns>
        public static IDataTable GetRequiredTable(IDataSet dataSet, string tableName)
        {
            // GetRequiredTable extension method call example
            IDataTable dataTable = dataSet.GetRequiredTable(tableName);

            return dataTable;
        }
        #endregion
    }
}
