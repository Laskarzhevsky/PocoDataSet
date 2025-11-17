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
        /// AddNewTableFromPocoInterface extension method example
        /// Adds new table from POCO interface to DataSet
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="interfaceType">Interface type</param>
        /// <returns>New table from POCO interface added to DataSet</returns>
        public static IDataTable AddNewTableFromPocoInterface(IDataSet dataSet, string tableName, Type interfaceType)
        {
            // AddNewTableFromPocoInterface extension method call example
            IDataTable dataTable = dataSet.AddNewTableFromPocoInterface(tableName, interfaceType);

            return dataTable;
        }
        #endregion
    }
}
