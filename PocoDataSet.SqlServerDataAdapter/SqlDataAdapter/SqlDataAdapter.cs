using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides SQL data adapter functionality
    /// </summary>
    public partial class SqlDataAdapter : AsyncDisposableObject
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public SqlDataAdapter(string? connectionString)
        {
            ConnectionString = connectionString;
            OwnsSqlConnection = true;
            ExecutionEngine = new SqlExecutionEngine(this);
        }

        /// <summary>
        /// Creates adapter that uses an already opened SQL connection.
        /// The adapter will not dispose this connection. This overload is useful for integration tests
        /// and for callers that want to control connection lifetime explicitly.
        /// </summary>
        /// <param name="sqlConnection">Existing SQL connection.</param>
        public SqlDataAdapter(SqlConnection sqlConnection)
        {
            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            SqlConnection = sqlConnection;
            ConnectionString = sqlConnection.ConnectionString;
            OwnsSqlConnection = false;
            ExecutionEngine = new SqlExecutionEngine(this);
        }
        #endregion


        #region Internal Properties
        /// <summary>
        /// Gets shared SQL execution engine (behavior); connection/transaction lifetime is owned by callers.
        /// </summary>
        internal SqlExecutionEngine ExecutionEngine
        {
            get;
        }

        /// <summary>
        /// Gets flag indicating whether this adapter created and owns the current SQL connection.
        /// </summary>
        internal bool OwnsSqlConnection
        {
            get;
            private set;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets flag indicating whether relations should be populated from database schema
        /// after filling a data set with multiple tables.
        /// </summary>
        public bool PopulateRelationsFromSchema
        {
            get; set;
        } = true;
        #endregion

        #region Public Methods
        /// <summary>
        /// Deletes data
        /// </summary>
        public async Task<int> DeleteAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            return await ExecuteNonQueryAsync(baseQuery, isStoredProcedure, parameters, connectionString);
        }


        /// <summary>
        /// Creates ADO.NET DataTables for generated Save stored procedures that accept SQL Server TVPs.
        /// Each returned table includes the __ChangeState metadata column used to classify
        /// Added / Modified / Deleted rows without relying on negative primary-key values.
        /// </summary>
        /// <param name="changeset">Source data set or changeset.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied. For search/read procedures pass false or omit the argument.</param>
        /// <returns>ADO.NET DataTables keyed by POCO table name.</returns>
        public Dictionary<string, System.Data.DataTable> CreateTableValuedParameterDataTables(IDataSet changeset, bool changedRowsOnly = true)
        {
            return SqlServerTableValuedParameterBuilder.CreateDataTables(changeset, changedRowsOnly);
        }

        /// <summary>
        /// Creates a structured SQL parameter for a generated table-valued parameter type using POCO columns plus adapter metadata columns.
        /// </summary>
        /// <param name="parameterName">SQL parameter name.</param>
        /// <param name="typeName">SQL Server table type name.</param>
        /// <param name="dataTable">Source POCO DataTable.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied. For search/read procedures pass false or omit the argument.</param>
        /// <returns>Configured SQL structured parameter.</returns>
        public SqlParameter CreateTableValuedParameter(string parameterName, string typeName, IDataTable dataTable, bool changedRowsOnly = false)
        {
            return SqlServerTableValuedParameterBuilder.CreateStructuredParameter(parameterName, typeName, dataTable, changedRowsOnly);
        }

        /// <summary>
        /// Creates a structured SQL parameter for a generated table-valued parameter type using the supplied SQL Server TVP schema.
        /// Missing POCO columns are sent as DBNull.Value. Extra POCO columns are ignored.
        /// </summary>
        /// <param name="parameterName">SQL parameter name.</param>
        /// <param name="typeName">SQL Server table type name.</param>
        /// <param name="dataTable">Source POCO DataTable.</param>
        /// <param name="columns">SQL Server TVP columns in exact SQL type order.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied. For search/read procedures pass false or omit the argument.</param>
        /// <returns>Configured SQL structured parameter.</returns>
        public SqlParameter CreateTableValuedParameter(string parameterName, string typeName, IDataTable dataTable, IEnumerable<SqlServerTableValuedParameterColumn> columns, bool changedRowsOnly = false)
        {
            return SqlServerTableValuedParameterBuilder.CreateStructuredParameter(parameterName, typeName, dataTable, columns, changedRowsOnly);
        }

        /// <summary>
        /// Creates a structured SQL parameter for a generated table-valued parameter type by requesting the SQL type column schema.
        /// This overload is useful when the POCO table contains only request/business columns, but the generated SQL type also contains standard persistence columns.
        /// </summary>
        /// <param name="parameterName">SQL parameter name.</param>
        /// <param name="typeName">SQL Server table type name.</param>
        /// <param name="dataTable">Source POCO DataTable.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied. For search/read procedures pass false or omit the argument.</param>
        /// <returns>Configured SQL structured parameter.</returns>
        public async Task<SqlParameter> CreateTableValuedParameterAsync(string parameterName, string typeName, IDataTable dataTable, bool changedRowsOnly = false)
        {
            SqlServerTableValuedParameterCreator creator = new SqlServerTableValuedParameterCreator();
            creator.LoadTableValuedParameterSchemaRequest += TableValuedParameterCreator_LoadTableValuedParameterSchemaRequestAsync;

            try
            {
                return await creator.CreateStructuredParameterAsync(parameterName, typeName, dataTable, changedRowsOnly).ConfigureAwait(false);
            }
            finally
            {
                creator.LoadTableValuedParameterSchemaRequest -= TableValuedParameterCreator_LoadTableValuedParameterSchemaRequestAsync;
            }
        }

        /// <summary>
        /// Creates a structured SQL parameter for a generated table-valued parameter type by requesting the SQL type column schema.
        /// Kept for compatibility with earlier adapter builds.
        /// </summary>
        /// <param name="parameterName">SQL parameter name.</param>
        /// <param name="typeName">SQL Server table type name.</param>
        /// <param name="dataTable">Source POCO DataTable.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied. For search/read procedures pass false or omit the argument.</param>
        /// <returns>Configured SQL structured parameter.</returns>
        public async Task<SqlParameter> CreateTableValuedParameterFromSqlTypeAsync(string parameterName, string typeName, IDataTable dataTable, bool changedRowsOnly = false)
        {
            return await CreateTableValuedParameterAsync(parameterName, typeName, dataTable, changedRowsOnly).ConfigureAwait(false);
        }

        /// <summary>
        /// Fills data set
        /// </summary>
        public async Task<IDataSet> FillAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, List<string>? returnedTableNames, string? connectionString, IDataSet? dataSet)
        {
            // TEST SEAM: bypass SQL Server when FillOverride is provided
            if (FillOverride != null)
            {
                return await FillOverride(baseQuery, isStoredProcedure, parameters, returnedTableNames, connectionString, dataSet).ConfigureAwait(false);
            }

            InitializeComponent(returnedTableNames);
            DataTableCreator!.DataSet = dataSet;

            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }

            CreateSqlCommand(baseQuery, isStoredProcedure);
            AddParametersToSqlCommand(parameters);

            try
            {
                await GetDataFromDatabaseAsync().ConfigureAwait(false);
                await DataTableCreator.AddTablesToDataSetAsync().ConfigureAwait(false);
                await CloseDataReaderAsync().ConfigureAwait(false);

                if (DataTableCreator.DataSet is not null)
                {
                    await RelationsManager.PopulateColumnMetadataFromDatabaseSchemaAsync(DataTableCreator.DataSet, SqlConnection!)
                        .ConfigureAwait(false);
                }

                if (PopulateRelationsFromSchema && DataTableCreator.DataSet is not null)
                {
                    await RelationsManager.PopulateRelationsFromDatabaseSchemaAsync(DataTableCreator.DataSet, SqlConnection!)
                        .ConfigureAwait(false);
                }

                dataSet = DataTableCreator.DataSet;
                dataSet!.AcceptChanges();
            }
            catch
            {
                throw;
            }
            finally
            {
                await DisposeAsync().ConfigureAwait(false);
            }

            return dataSet!;
        }

        /// <summary>
        /// Fills data set using provider-specific SQL parameters.
        /// Use this overload for table-valued parameters and other parameters that need
        /// SqlParameter-specific configuration.
        /// </summary>
        public async Task<IDataSet> FillAsync(string baseQuery, bool isStoredProcedure, params SqlParameter[] parameters)
        {
            return await FillAsync(baseQuery, isStoredProcedure, parameters, null, null, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Fills data set using provider-specific SQL parameters.
        /// </summary>
        public async Task<IDataSet> FillAsync(string baseQuery, bool isStoredProcedure, SqlParameter[]? parameters, List<string>? returnedTableNames, string? connectionString, IDataSet? dataSet)
        {
            // TEST SEAM: bypass SQL Server when FillWithSqlParametersOverride is provided
            if (FillWithSqlParametersOverride != null)
            {
                return await FillWithSqlParametersOverride(baseQuery, isStoredProcedure, parameters, returnedTableNames, connectionString, dataSet).ConfigureAwait(false);
            }

            InitializeComponent(returnedTableNames);
            DataTableCreator!.DataSet = dataSet;

            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }

            CreateSqlCommand(baseQuery, isStoredProcedure);
            AddParametersToSqlCommand(parameters);

            try
            {
                await GetDataFromDatabaseAsync().ConfigureAwait(false);
                await DataTableCreator.AddTablesToDataSetAsync().ConfigureAwait(false);
                await CloseDataReaderAsync().ConfigureAwait(false);

                if (DataTableCreator.DataSet is not null)
                {
                    await RelationsManager.PopulateColumnMetadataFromDatabaseSchemaAsync(DataTableCreator.DataSet, SqlConnection!)
                        .ConfigureAwait(false);
                }

                if (PopulateRelationsFromSchema && DataTableCreator.DataSet is not null)
                {
                    await RelationsManager.PopulateRelationsFromDatabaseSchemaAsync(DataTableCreator.DataSet, SqlConnection!)
                        .ConfigureAwait(false);
                }

                dataSet = DataTableCreator.DataSet;
                dataSet!.AcceptChanges();
            }
            catch
            {
                throw;
            }
            finally
            {
                await DisposeAsync().ConfigureAwait(false);
            }

            return dataSet!;
        }


        /// <summary>
        /// Fills data set using table-valued parameter descriptions.
        /// The adapter opens one connection, creates the TVP parameters on that connection,
        /// executes the command, reads data, closes the reader, loads metadata, and then disposes the connection.
        /// </summary>
        public async Task<IDataSet> FillAsync(string baseQuery, bool isStoredProcedure, SqlTableValuedParameterInfo[]? tableValuedParameters, List<string>? returnedTableNames, string? connectionString, IDataSet? dataSet)
        {
            InitializeComponent(returnedTableNames);
            DataTableCreator!.DataSet = dataSet;

            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }

            CreateSqlCommand(baseQuery, isStoredProcedure);

            try
            {
                await OpenSqlConnectionAsync().ConfigureAwait(false);

                SqlParameter[] sqlParameters = await CreateTableValuedParametersAsync(tableValuedParameters).ConfigureAwait(false);
                AddParametersToSqlCommand(sqlParameters);

                await ExecuteReaderOnOpenConnectionAsync().ConfigureAwait(false);
                await DataTableCreator.AddTablesToDataSetAsync().ConfigureAwait(false);
                await CloseDataReaderAsync().ConfigureAwait(false);

                if (DataTableCreator.DataSet is not null)
                {
                    await RelationsManager.PopulateColumnMetadataFromDatabaseSchemaAsync(DataTableCreator.DataSet, SqlConnection!)
                        .ConfigureAwait(false);
                }

                if (PopulateRelationsFromSchema && DataTableCreator.DataSet is not null)
                {
                    await RelationsManager.PopulateRelationsFromDatabaseSchemaAsync(DataTableCreator.DataSet, SqlConnection!)
                        .ConfigureAwait(false);
                }

                dataSet = DataTableCreator.DataSet;
                dataSet!.AcceptChanges();
            }
            catch
            {
                throw;
            }
            finally
            {
                await DisposeAsync().ConfigureAwait(false);
            }

            return dataSet!;
        }


        /// <summary>
        /// Fills an existing data set by adding one or more result-set tables into it.
        /// </summary>
        public IDataSet FillIntoExistingDataSet(IDataSet dataSet, string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, List<string>? returnedTableNames, string? connectionString)
        {
            return FillIntoExistingDataSetAsync(dataSet, baseQuery, isStoredProcedure, parameters, returnedTableNames, connectionString)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Fills an existing data set by adding one or more result-set tables into it.
        /// </summary>
        public async Task<IDataSet> FillIntoExistingDataSetAsync(IDataSet dataSet, string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, List<string>? returnedTableNames, string? connectionString)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException(nameof(dataSet));
            }

            return await FillAsync(baseQuery, isStoredProcedure, parameters, returnedTableNames, connectionString, dataSet)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fills an existing data set by adding one or more result-set tables into it using provider-specific SQL parameters.
        /// </summary>
        public IDataSet FillIntoExistingDataSet(IDataSet dataSet, string baseQuery, bool isStoredProcedure, params SqlParameter[] parameters)
        {
            return FillIntoExistingDataSetAsync(dataSet, baseQuery, isStoredProcedure, parameters)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Fills an existing data set by adding one or more result-set tables into it using provider-specific SQL parameters.
        /// </summary>
        public async Task<IDataSet> FillIntoExistingDataSetAsync(IDataSet dataSet, string baseQuery, bool isStoredProcedure, params SqlParameter[] parameters)
        {
            return await FillIntoExistingDataSetAsync(dataSet, baseQuery, isStoredProcedure, parameters, null, null)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fills an existing data set by adding one or more result-set tables into it using provider-specific SQL parameters.
        /// </summary>
        public async Task<IDataSet> FillIntoExistingDataSetAsync(IDataSet dataSet, string baseQuery, bool isStoredProcedure, SqlParameter[]? parameters, List<string>? returnedTableNames, string? connectionString)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException(nameof(dataSet));
            }

            return await FillAsync(baseQuery, isStoredProcedure, parameters, returnedTableNames, connectionString, dataSet)
                .ConfigureAwait(false);
        }


        /// <summary>
        /// Fills an existing data set by adding one or more result-set tables into it using table-valued parameter descriptions.
        /// </summary>
        public async Task<IDataSet> FillIntoExistingDataSetAsync(IDataSet dataSet, string baseQuery, bool isStoredProcedure, SqlTableValuedParameterInfo[]? tableValuedParameters, List<string>? returnedTableNames, string? connectionString)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException(nameof(dataSet));
            }

            return await FillAsync(baseQuery, isStoredProcedure, tableValuedParameters, returnedTableNames, connectionString, dataSet)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a non-query command using table-valued parameter descriptions.
        /// The adapter creates the TVP parameters inside the same connection that executes the command.
        /// </summary>
        /// <param name="baseQuery">Base query or stored procedure name.</param>
        /// <param name="isStoredProcedure">Flag indicating whether base query is stored procedure.</param>
        /// <param name="tableValuedParameters">Table-valued parameter descriptions.</param>
        /// <param name="connectionString">Optional connection string override.</param>
        /// <returns>Execution result.</returns>
        public async Task<int> ExecuteNonQueryAsync(string baseQuery, bool isStoredProcedure, SqlTableValuedParameterInfo[]? tableValuedParameters, string? connectionString = null)
        {
            return await ExecuteNonQueryWithTableValuedParametersAsync(baseQuery, isStoredProcedure, tableValuedParameters, null, connectionString).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a non-query command using table-valued parameter descriptions and scalar parameters.
        /// The adapter creates the TVP parameters inside the same connection that executes the command.
        /// </summary>
        /// <param name="baseQuery">Base query or stored procedure name.</param>
        /// <param name="isStoredProcedure">Flag indicating whether base query is stored procedure.</param>
        /// <param name="tableValuedParameters">Table-valued parameter descriptions.</param>
        /// <param name="parameters">Scalar query parameters.</param>
        /// <param name="connectionString">Optional connection string override.</param>
        /// <returns>Execution result.</returns>
        public async Task<int> ExecuteNonQueryAsync(string baseQuery, bool isStoredProcedure, SqlTableValuedParameterInfo[]? tableValuedParameters, Dictionary<string, object?>? parameters, string? connectionString = null)
        {
            return await ExecuteNonQueryWithTableValuedParametersAsync(baseQuery, isStoredProcedure, tableValuedParameters, parameters, connectionString).ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts data
        /// </summary>
        public async Task<int> InsertAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            return await ExecuteNonQueryAsync(baseQuery, isStoredProcedure, parameters, connectionString);
        }

        /// <summary>
        /// Updates data
        /// </summary>
        public async Task<int> UpdateAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            return await ExecuteNonQueryAsync(baseQuery, isStoredProcedure, parameters, connectionString);
        }

        /// <summary>
        /// Saves data set changes to SQL Server.
        /// </summary>
        /// <summary>
        /// Begins a SQL Server transaction scope.
        /// </summary>
        /// <param name="connectionString">Optional connection string override</param>
        public async Task<SqlDataAdapterTransaction> BeginTransactionAsync(string? connectionString = null)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }

            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new InvalidOperationException("ConnectionString is not set.");
            }

            SqlConnection sqlConnection;
            bool ownsConnection;

            if (SqlConnection != null)
            {
                sqlConnection = SqlConnection;
                ownsConnection = false;
            }
            else
            {
                sqlConnection = new SqlConnection(ConnectionString);
                ownsConnection = true;
            }

            if (sqlConnection.State != System.Data.ConnectionState.Open)
            {
                await sqlConnection.OpenAsync().ConfigureAwait(false);
            }

            SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

            return new SqlDataAdapterTransaction(this, sqlConnection, sqlTransaction, ownsConnection);
        }

        /// <summary>
        /// Saves data set changes to SQL Server.
        /// </summary>
        public async Task<int> SaveChangesAsync(IDataSet changeset, string? connectionString = null)
        {
            return await SaveChangesAsync(changeset, null, connectionString).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves data set changes to SQL Server using the requested save options.
        /// </summary>
        public async Task<int> SaveChangesAsync(IDataSet changeset, SqlDataAdapterOptions? options, string? connectionString = null)
        {
            if (changeset == null)
            {
                throw new ArgumentNullException(nameof(changeset));
            }

            // No-op for empty changeset (no DB work, no ConnectionString required).
            if (changeset.Tables == null || changeset.Tables.Count == 0)
            {
                return 0;
            }

            SqlDataAdapterOptions effectiveOptions = ResolveOptions(options);

            SqlDataAdapterTransaction? transaction = null;

            try
            {
                transaction = await BeginTransactionAsync(connectionString).ConfigureAwait(false);

                int affectedRows = await transaction.SaveChangesAsync(changeset, effectiveOptions).ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);

                return affectedRows;
            }
            catch
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                }

                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Saves data set changes to SQL Server and merges post-save values.
        /// </summary>
        public async Task<int> SaveChangesAndMergePostSaveAsync(IDataSet changeset, string? connectionString = null)
        {
            return await SaveChangesAndMergePostSaveAsync(changeset, null, connectionString).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves data set changes to SQL Server using the requested save options and merges post-save values.
        /// </summary>
        public async Task<int> SaveChangesAndMergePostSaveAsync(IDataSet changeset, SqlDataAdapterOptions? options, string? connectionString = null)
        {
            int affectedRows = await SaveChangesAsync(changeset, options, connectionString).ConfigureAwait(false);

            IMergeOptions mergeOptions = new MergeOptions();
            changeset.DoPostSaveMerge(changeset, mergeOptions);

            return affectedRows;
        }

        static SqlDataAdapterOptions ResolveOptions(SqlDataAdapterOptions? options)
        {
            if (options == null)
            {
                return new SqlDataAdapterOptions();
            }

            return options;
        }
        #endregion

        #region Internal Test Hook
        /// <summary>
        /// INTERNAL TEST HOOK.
        /// Allows tests to bypass SQL Server access and provide an in-memory implementation
        /// of FillAsync while still exercising the real public API.
        /// This delegate MUST remain null in production.
        /// </summary>
        internal delegate Task<IDataSet> FillOverrideDelegate(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, List<string>? returnedTableNames, string? connectionString, IDataSet? existingDataSet);

        /// <summary>
        /// INTERNAL TEST HOOK.
        /// When set, FillAsync delegates execution to this override instead of accessing SQL Server.
        /// </summary>
        internal FillOverrideDelegate? FillOverride
        {
            get; set;
        }

        /// <summary>
        /// INTERNAL TEST HOOK.
        /// Allows tests to bypass SQL Server access when the SqlParameter overloads are used.
        /// This delegate MUST remain null in production.
        /// </summary>
        internal delegate Task<IDataSet> FillWithSqlParametersOverrideDelegate(string baseQuery, bool isStoredProcedure, SqlParameter[]? parameters, List<string>? returnedTableNames, string? connectionString, IDataSet? existingDataSet);

        /// <summary>
        /// INTERNAL TEST HOOK.
        /// When set, provider-specific FillAsync overloads delegate execution to this override instead of accessing SQL Server.
        /// </summary>
        internal FillWithSqlParametersOverrideDelegate? FillWithSqlParametersOverride
        {
            get; set;
        }
        #endregion
    }
}
