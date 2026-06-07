using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.Extensions;
using PocoDataSet.Extensions.Relations;
using PocoDataSet.IData;

using PocoDataRowState = PocoDataSet.IData.DataRowState;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides SQL data adapter functionality
    /// </summary>
    public partial class SqlDataAdapter
    {
        #region Internal Connection
        /// <summary>
        /// Gets current SQL connection for the active operation (internal for tests).
        /// </summary>
        internal SqlConnection? SqlConnection
        {
            get; set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds parameters to SQL command
        /// </summary>
        /// <param name="parameters">Parameters to add to SQL command</param>
        internal void AddParametersToSqlCommand(Dictionary<string, object?>? parameters)
        {
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object?> parameter in parameters)
                {
                    SqlParameter sqlParameter = SqlCommand!.CreateParameter();
                    sqlParameter.ParameterName = parameter.Key;
                    if (parameter.Value == null)
                    {
                        sqlParameter.Value = DBNull.Value;
                    }
                    else
                    {
                        sqlParameter.Value = parameter.Value;
                    }

                    SqlCommand.Parameters.Add(sqlParameter);
                }
            }
        }


        /// <summary>
        /// Adds SQL parameters to SQL command. Use this overload for parameters that require
        /// provider-specific settings such as SqlDbType.Structured and TypeName.
        /// </summary>
        /// <param name="parameters">SQL parameters to add to SQL command</param>
        internal void AddParametersToSqlCommand(SqlParameter[]? parameters)
        {
            if (parameters == null)
            {
                return;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                SqlParameter parameter = parameters[i];
                if (parameter == null)
                {
                    continue;
                }

                SqlCommand!.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// Creates SQL command
        /// </summary>
        /// <param name="baseQuery">Base query</param>
        /// <param name="isStoredProcedure">Flag indicating whether base query is stored procedure</param>
        internal void CreateSqlCommand(string baseQuery, bool isStoredProcedure)
        {
            SqlCommand = new SqlCommand();
            SqlCommand.CommandText = baseQuery;
            if (isStoredProcedure)
            {
                SqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            }
            else
            {
                SqlCommand.CommandType = System.Data.CommandType.Text;
            }
        }

        /// <summary>
        /// Executes non-query asynchronously
        /// </summary>
        /// <param name="baseQuery">Base query</param>
        /// <param name="isStoredProcedure">Flag indicating whether base query is stored procedure</param>
        /// <param name="parameters">Query parameters</param>
        /// <returns>Execution result</returns>
        async Task<int> ExecuteNonQueryAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }
            try
            {
                await using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    return await ExecutionEngine.ExecuteNonQueryAsync(
                        baseQuery,
                        isStoredProcedure,
                        parameters,
                        sqlConnection,
                        null).ConfigureAwait(false);
                }
            }
            finally
            {
                await DisposeAsync();
            }
        }

        /// <summary>
        /// Gets data from database
        /// </summary>
        internal async Task GetDataFromDatabaseAsync()
        {
            SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            SqlConnection = sqlConnection;

            SqlCommand!.Connection = sqlConnection;
            await sqlConnection.OpenAsync().ConfigureAwait(false);
            DataTableCreator!.SqlDataReader = await SqlCommand!.ExecuteReaderAsync().ConfigureAwait(false);
        }

/// <summary>
/// Gets data from database using an existing open connection and optional transaction.
/// </summary>
internal async Task GetDataFromDatabaseAsync(SqlConnection sqlConnection, SqlTransaction? sqlTransaction)
{
    if (sqlConnection == null)
    {
        throw new ArgumentNullException(nameof(sqlConnection));
    }

    SqlConnection = sqlConnection;

    SqlCommand!.Connection = sqlConnection;
    SqlCommand.Transaction = sqlTransaction;

    if (sqlConnection.State != System.Data.ConnectionState.Open)
    {
        await sqlConnection.OpenAsync().ConfigureAwait(false);
    }

    DataTableCreator!.SqlDataReader = await SqlCommand!.ExecuteReaderAsync().ConfigureAwait(false);
}


        /// <summary>
        /// Initializes component
        /// <param name="returnedTableNames">Returned table names</param>
        /// </summary>
        internal void InitializeComponent(List<string>? returnedTableNames)
        {
            DataTableCreator = new DataTableCreator();
            DataTableCreator.ListOfTableNames = returnedTableNames;
            DataTableCreator.LoadDataTableKeysInformationRequest += DataTableCreator_LoadDataTableKeysInformationRequestAsync;
        }

        /// <summary>
        /// Releases resources
        /// </summary>
        protected override void ReleaseResources()
        {
            // event unsubscriptions & synchronous nulling
            if (DataTableCreator is not null)
            {
                DataTableCreator.LoadDataTableKeysInformationRequest -= DataTableCreator_LoadDataTableKeysInformationRequestAsync;
                DataTableCreator.DataSet = null;
                DataTableCreator = null;
            }
        }

        /// <summary>
        /// Releases resources asynchronously
        /// </summary>
        protected override async ValueTask ReleaseResourcesAsync()
        {
            if (DataTableCreator?.SqlDataReader is not null)
            {
                await DataTableCreator.SqlDataReader.DisposeAsync().ConfigureAwait(false);
                DataTableCreator.SqlDataReader = null;
            }

            if (SqlConnection is not null)
            {
                await SqlConnection.DisposeAsync().ConfigureAwait(false);
                SqlConnection = null;
            }

            if (SqlCommand is not null)
            {
                SqlCommand.Connection = null;
                await SqlCommand.DisposeAsync().ConfigureAwait(false);
                SqlCommand = null;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets connection string
        /// </summary>
        string? ConnectionString
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets data table creator
        /// </summary>
        public DataTableCreator? DataTableCreator
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets SQL command
        /// </summary>
        internal SqlCommand? SqlCommand
        {
            get; set;
        }
        /// <summary>
        /// Gets or sets SQL transaction
        /// </summary>
        internal SqlTransaction? SqlTransaction
        {
            get; set;
        }

        internal Task<int> ExecuteNonQueryAsync(SqlCommand sqlCommand)
        {
            if (sqlCommand == null)
            {
                throw new ArgumentNullException(nameof(sqlCommand));
            }

            if (SqlConnection == null)
            {
                throw new InvalidOperationException("No active SaveChanges connection. This method may only be used during SaveChangesAsync.");
            }

            sqlCommand.Connection = SqlConnection;
            sqlCommand.Transaction = SqlTransaction;

            return sqlCommand.ExecuteNonQueryAsync();
        }

        internal Task<SqlDataReader> ExecuteReaderAsync(SqlCommand sqlCommand)
        {
            if (sqlCommand == null)
            {
                throw new ArgumentNullException(nameof(sqlCommand));
            }

            if (SqlConnection == null)
            {
                throw new InvalidOperationException("No active SaveChanges connection. This method may only be used during SaveChangesAsync.");
            }

            sqlCommand.Connection = SqlConnection;
            sqlCommand.Transaction = SqlTransaction;

            return sqlCommand.ExecuteReaderAsync();
        }

        internal Task<TableWriteMetadata> GetOrLoadTableWriteMetadataAsync(
            string tableName,
            Dictionary<string, TableWriteMetadata> metadataCache)
        {
            if (SqlConnection == null)
            {
                throw new InvalidOperationException("No active SaveChanges connection. This method may only be used during SaveChangesAsync.");
            }

            return MetadataLoader.GetOrLoadTableWriteMetadataAsync(metadataCache, tableName, SqlConnection, SqlTransaction);
        }

        internal Task<List<ForeignKeyEdge>> LoadForeignKeyEdgesAsync(IDataSet changeset, List<IDataTable> tablesWithChanges, HashSet<string> namesOfTablesWithChanges)
        {
            if (SqlConnection == null)
            {
                throw new InvalidOperationException("No active SaveChanges connection. This method may only be used during SaveChangesAsync.");
            }

            if (SqlTransaction == null)
            {
                throw new InvalidOperationException("No active SaveChanges transaction. This method may only be used during SaveChangesAsync.");
            }

            return MetadataLoader.LoadForeignKeyEdgesAsync(namesOfTablesWithChanges, SqlConnection, SqlTransaction);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles SqlServerTableValuedParameterCreator.LoadTableValuedParameterSchemaRequest event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        async Task TableValuedParameterCreator_LoadTableValuedParameterSchemaRequestAsync(object? sender, LoadTableValuedParameterSchemaEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            e.Columns = await LoadTableValuedParameterColumnsAsync(e.TypeName).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads SQL Server table-valued parameter column metadata for a user-defined table type.
        /// </summary>
        /// <param name="typeName">SQL Server table type name.</param>
        /// <returns>TVP columns in SQL Server column order.</returns>
        async Task<List<SqlServerTableValuedParameterColumn>> LoadTableValuedParameterColumnsAsync(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentException("SQL Server table type name is required.", nameof(typeName));
            }

            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new InvalidOperationException("Connection string is required to load SQL Server table-valued parameter schema.");
            }

            string schemaName;
            string tableTypeName;
            SplitSqlTypeName(typeName, out schemaName, out tableTypeName);

            List<SqlServerTableValuedParameterColumn> columns = new List<SqlServerTableValuedParameterColumn>();

            await using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                await sqlConnection.OpenAsync().ConfigureAwait(false);

                await using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText =
@"select
    c.[name] as ColumnName,
    st.[name] as DataType
from
    sys.table_types tt
    inner join sys.schemas s on tt.[schema_id] = s.[schema_id]
    inner join sys.columns c on tt.type_table_object_id = c.[object_id]
    inner join sys.types st on c.system_type_id = st.system_type_id and st.system_type_id = st.user_type_id
where
    s.[name] = @SchemaName
    and tt.[name] = @TypeName
order by
    c.column_id";

                    sqlCommand.Parameters.AddWithValue("@SchemaName", schemaName);
                    sqlCommand.Parameters.AddWithValue("@TypeName", tableTypeName);

                    await using (SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await sqlDataReader.ReadAsync().ConfigureAwait(false))
                        {
                            string columnName = sqlDataReader.GetString(0);
                            string dataType = sqlDataReader.GetString(1);
                            columns.Add(new SqlServerTableValuedParameterColumn(columnName, dataType));
                        }
                    }
                }
            }

            if (columns.Count == 0)
            {
                throw new InvalidOperationException("SQL Server table-valued parameter type was not found: " + typeName);
            }

            return columns;
        }

        /// <summary>
        /// Splits schema-qualified SQL type name into schema and type name.
        /// </summary>
        /// <param name="typeName">SQL type name.</param>
        /// <param name="schemaName">Schema name.</param>
        /// <param name="tableTypeName">Table type name.</param>
        static void SplitSqlTypeName(string typeName, out string schemaName, out string tableTypeName)
        {
            string normalized = typeName.Trim();
            normalized = normalized.Replace("[", string.Empty);
            normalized = normalized.Replace("]", string.Empty);

            int dotIndex = normalized.LastIndexOf('.');
            if (dotIndex < 0)
            {
                schemaName = "dbo";
                tableTypeName = normalized;
                return;
            }

            schemaName = normalized.Substring(0, dotIndex);
            tableTypeName = normalized.Substring(dotIndex + 1);
        }

        /// <summary>
        /// Handles DataTableCreator.LoadDataTableKeysInformationRequest event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        async Task DataTableCreator_LoadDataTableKeysInformationRequestAsync(object? sender, EventArgs e)
        {
            if (DataTableCreator == null || SqlConnection == null)
            {
                return;
            }

            await RelationsManager.LoadDataTablePrimaryKeysAsync(DataTableCreator, SqlConnection).ConfigureAwait(false);
            await RelationsManager.LoadDataTableForeignKeysAsync(DataTableCreator, SqlConnection).ConfigureAwait(false);
        }
        #endregion
    }
}
