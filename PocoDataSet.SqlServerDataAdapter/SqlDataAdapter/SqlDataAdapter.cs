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
                        if (changeset == null)
            {
                throw new ArgumentNullException(nameof(changeset));
            }

            // No-op for empty changeset (no DB work, no ConnectionString required).
            if (changeset.Tables == null || changeset.Tables.Count == 0)
            {
                return 0;
            }

SqlDataAdapterTransaction? transaction = null;

            try
            {
                transaction = await BeginTransactionAsync(connectionString).ConfigureAwait(false);

                int affectedRows = await transaction.SaveChangesAsync(changeset).ConfigureAwait(false);
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
            int affectedRows = await SaveChangesAsync(changeset, connectionString);
            changeset.MergeWith(changeset, MergeMode.PostSave);

            return affectedRows;
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
