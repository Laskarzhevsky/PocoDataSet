using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Demo.DataSetExtensions
{
    /// <summary>
    /// Provides examples of POCO DataSet functionality
    /// </summary>
    internal static partial class DataTableExtensionExamples
    {
        #region Public Methods
        /// <summary>
        /// Provides an example of calling the AddColumnsFromInterface extension method
        /// Add columns to data table from interface
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        public static void AddColumnsFromInterface(IDataTable? dataTable, Type interfaceType)
        {
            // AddColumnsFromInterface extension method call example
            dataTable.AddColumnsFromInterface(interfaceType);
        }
        #endregion
    }
}
