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
        /// Provides an example of calling the AddNewRow extension method
        /// Creates new data row in data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>New data row created in data table</returns>
        public static IDataRow AddNewRow(IDataTable dataTable)
        {
            // AddNewRow extension method call example
            IDataRow dataRow = dataTable.AddNewRow();

            return dataRow;
        }
        #endregion
    }
}
