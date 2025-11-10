using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Adds column
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        public static void AddColumn(this IDataTable dataTable, string columnName, string dataType)
        {
            IColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = columnName;
            columnMetadata.DataType = dataType;
            dataTable.Columns.Add(columnMetadata);
        }

        /// <summary>
        /// Adds column
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        public static void AddColumn(this IDataTable dataTable, string columnName, string dataType, bool isPrimaryKey)
        {
            IColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = columnName;
            columnMetadata.DataType = dataType;
            columnMetadata.IsPrimaryKey = isPrimaryKey;
            dataTable.Columns.Add(columnMetadata);
        }

        /// <summary>
        /// Adds column
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        /// <param name="isForeignKey">Flag indicating whether column is foreign key</param>
        public static void AddColumn(this IDataTable dataTable, string columnName, string dataType, bool isPrimaryKey, bool isForeignKey)
        {
            IColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = columnName;
            columnMetadata.DataType = dataType;
            columnMetadata.IsPrimaryKey = isPrimaryKey;
            columnMetadata.IsForeignKey = isForeignKey;
            dataTable.Columns.Add(columnMetadata);
        }
        #endregion
    }
}
