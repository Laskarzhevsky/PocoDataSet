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
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied.</param>
        /// <returns>ADO.NET DataTables keyed by POCO table name.</returns>
        public Dictionary<string, System.Data.DataTable> CreateTableValuedParameterDataTables(IDataSet changeset, bool changedRowsOnly = true)
        {
            return SqlServerTableValuedParameterBuilder.CreateDataTables(changeset, changedRowsOnly);
        }

        /// <summary>
        /// Creates a structured SQL parameter for a generated Save stored procedure.
        /// </summary>
        /// <param name="parameterName">SQL parameter name.</param>
        /// <param name="typeName">SQL Server table type name.</param>
        /// <param name="dataTable">Source POCO DataTable.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied.</param>
        /// <returns>Configured SQL structured parameter.</returns>
        public SqlParameter CreateTableValuedParameter(string parameterName, string typeName, IDataTable dataTable, bool changedRowsOnly = true)
        {
            return SqlServerTableValuedParameterBuilder.CreateStructuredParameter(parameterName, typeName, dataTable, changedRowsOnly);
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

            SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            await sqlConnection.OpenAsync().ConfigureAwait(false);

            SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

            return new SqlDataAdapterTransaction(this, sqlConnection, sqlTransaction, true);
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
        #endregion
    }
}
