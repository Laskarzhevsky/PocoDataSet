using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Coordinates SQL Server table-valued parameter creation.
    /// The creator does not load database metadata itself; it requests schema metadata from its owner.
    /// </summary>
    internal sealed class SqlServerTableValuedParameterCreator
    {
        #region Events
        /// <summary>
        /// Requests SQL Server table-valued parameter schema metadata.
        /// </summary>
        internal event AsyncEventHandler<LoadTableValuedParameterSchemaEventArgs>? LoadTableValuedParameterSchemaRequest;
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a structured SQL parameter using the SQL Server table type schema returned by the owner.
        /// </summary>
        /// <param name="parameterName">SQL parameter name.</param>
        /// <param name="typeName">SQL Server table type name.</param>
        /// <param name="dataTable">Source POCO DataTable.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied.</param>
        /// <returns>Configured SQL structured parameter.</returns>
        public async Task<SqlParameter> CreateStructuredParameterAsync(string parameterName, string typeName, IDataTable dataTable, bool changedRowsOnly = false)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentException("SQL Server table type name is required.", nameof(typeName));
            }

            LoadTableValuedParameterSchemaEventArgs args = new LoadTableValuedParameterSchemaEventArgs(typeName);
            await LoadTableValuedParameterSchemaAsync(args).ConfigureAwait(false);

            if (args.Columns == null || args.Columns.Count == 0)
            {
                throw new InvalidOperationException("SQL Server table-valued parameter type was not found: " + typeName);
            }

            return SqlServerTableValuedParameterBuilder.CreateStructuredParameter(
                parameterName,
                typeName,
                dataTable,
                args.Columns,
                changedRowsOnly);
        }
        #endregion

        #region Private Methods
        async Task LoadTableValuedParameterSchemaAsync(LoadTableValuedParameterSchemaEventArgs args)
        {
            if (LoadTableValuedParameterSchemaRequest == null)
            {
                throw new InvalidOperationException("No handler was registered for SQL Server table-valued parameter schema loading.");
            }

            await LoadTableValuedParameterSchemaRequest(this, args).ConfigureAwait(false);
        }
        #endregion
    }
}
