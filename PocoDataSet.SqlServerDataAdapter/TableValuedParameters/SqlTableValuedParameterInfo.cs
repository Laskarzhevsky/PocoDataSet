using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Describes a SQL Server table-valued parameter that should be created inside
    /// the adapter operation, using the operation connection.
    /// </summary>
    public sealed class SqlTableValuedParameterInfo
    {
        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameterName">SQL parameter name.</param>
        /// <param name="typeName">SQL Server table type name.</param>
        /// <param name="dataTable">Source POCO DataTable.</param>
        public SqlTableValuedParameterInfo(string parameterName, string typeName, IDataTable dataTable)
            : this(parameterName, typeName, dataTable, false)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameterName">SQL parameter name.</param>
        /// <param name="typeName">SQL Server table type name.</param>
        /// <param name="dataTable">Source POCO DataTable.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied.</param>
        public SqlTableValuedParameterInfo(string parameterName, string typeName, IDataTable dataTable, bool changedRowsOnly)
        {
            ParameterName = parameterName;
            TypeName = typeName;
            DataTable = dataTable;
            ChangedRowsOnly = changedRowsOnly;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets SQL parameter name.
        /// </summary>
        public string ParameterName
        {
            get;
        }

        /// <summary>
        /// Gets SQL Server table type name.
        /// </summary>
        public string TypeName
        {
            get;
        }

        /// <summary>
        /// Gets source POCO DataTable.
        /// </summary>
        public IDataTable DataTable
        {
            get;
        }

        /// <summary>
        /// Gets flag indicating whether only changed rows should be copied.
        /// </summary>
        public bool ChangedRowsOnly
        {
            get;
        }
        #endregion
    }
}
