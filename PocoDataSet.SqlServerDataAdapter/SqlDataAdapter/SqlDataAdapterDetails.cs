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
