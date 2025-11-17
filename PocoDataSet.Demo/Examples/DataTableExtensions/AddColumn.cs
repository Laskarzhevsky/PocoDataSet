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
        /// Provides an example of calling the AddColumn extension method
        /// Add column to data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        public static void AddColumn(IDataTable dataTable, string columnName, string dataType)
        {
            // AddColumn extension method call example
            dataTable.AddColumn(columnName, dataType);
        }

        /// <summary>
        /// Provides an example of calling the AddColumn extension method
        /// Add column to data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        public static void AddColumn(IDataTable dataTable, string columnName, string dataType, bool isPrimaryKey)
        {
            // AddColumn extension method call example
            dataTable.AddColumn(columnName, dataType, isPrimaryKey);
        }

        /// <summary>
        /// Provides an example of calling the AddColumn extension method
        /// Add column to data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        /// <param name="isForeignKey">Flag indicating whether column is foreign key</param>
        public static void AddColumn(IDataTable dataTable, string columnName, string dataType, bool isPrimaryKey, bool isForeignKey)
        {
            // AddColumn extension method call example
            dataTable.AddColumn(columnName, dataType, isPrimaryKey, isForeignKey);
        }
        #endregion
    }
}
