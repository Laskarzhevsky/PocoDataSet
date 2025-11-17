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
        /// AddNewTable extension method example
        /// Adds new table to DataSet
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <returns>New table added to DataSet</returns>
        public static IDataTable AddNewTable(IDataSet dataSet, string tableName)
        {
            // AddNewTable extension method call example
            IDataTable dataTable = dataSet.AddNewTable(tableName);

            return dataTable;
        }

        /// <summary>
        /// AddNewTable extension method example
        /// Adds new table to DataSet
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>New table added to DataSet</returns>
        public static IDataTable AddNewTable(IDataSet dataSet, string tableName, List<IColumnMetadata> listOfColumnMetadata)
        {
            // AddNewTable extension method call example
            IDataTable dataTable = dataSet.AddNewTable(tableName, listOfColumnMetadata);

            return dataTable;
        }
        #endregion
    }
}
