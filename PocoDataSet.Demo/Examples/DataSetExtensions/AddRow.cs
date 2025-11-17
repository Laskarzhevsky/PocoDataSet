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
        /// AddRow extension method example
        /// Adds row to table
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="dataRow">Data row for addition</param>
        public static void AddRow(IDataSet dataSet, string tableName, IDataRow dataRow)
        {
            // AddRow extension method call example
            dataSet.AddRow(tableName, dataRow);
        }
        #endregion
    }
}
