using System;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Describes one SQL Server table-valued parameter column.
    /// </summary>
    public sealed class SqlServerTableValuedParameterColumn
    {
        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlServerTableValuedParameterColumn()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <param name="dataType">SQL Server data type name.</param>
        public SqlServerTableValuedParameterColumn(string columnName, string dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets column name.
        /// </summary>
        public string ColumnName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets SQL Server data type name.
        /// </summary>
        public string DataType
        {
            get; set;
        } = string.Empty;
        #endregion
    }
}
