using System;
using System.Collections.Generic;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides event arguments for loading SQL Server table-valued parameter schema metadata.
    /// </summary>
    internal sealed class LoadTableValuedParameterSchemaEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeName">SQL Server table-valued parameter type name.</param>
        public LoadTableValuedParameterSchemaEventArgs(string typeName)
        {
            TypeName = typeName;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets SQL Server table-valued parameter type name.
        /// </summary>
        public string TypeName
        {
            get;
        }

        /// <summary>
        /// Gets or sets loaded SQL Server table-valued parameter columns in SQL Server column order.
        /// </summary>
        public List<SqlServerTableValuedParameterColumn>? Columns
        {
            get; set;
        }
        #endregion
    }
}
