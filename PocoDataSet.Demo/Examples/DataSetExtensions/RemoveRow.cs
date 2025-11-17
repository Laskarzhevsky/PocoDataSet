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
        /// RemoveRow extension method example
        /// Removes row from table
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        public static void RemoveRow(IDataSet dataSet, string tableName, int rowIndex)
        {
            // RemoveRow extension method call example
            dataSet.RemoveRow(tableName, rowIndex);
        }
        #endregion
    }
}
