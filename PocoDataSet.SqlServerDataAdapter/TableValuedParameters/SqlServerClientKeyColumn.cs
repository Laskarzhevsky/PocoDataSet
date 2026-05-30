using System;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Defines SQL Server adapter client-correlation metadata column used by generated table-valued parameter types.
    /// </summary>
    public static class SqlServerClientKeyColumn
    {
        #region Constants
        /// <summary>
        /// Client key metadata column name.
        /// </summary>
        public const string ColumnName = "__ClientKey";
        #endregion
    }
}
