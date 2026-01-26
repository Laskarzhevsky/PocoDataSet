using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.Data;
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
        void AddParametersToSqlCommand(Dictionary<string, object?>? parameters)
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
        void CreateSqlCommand(string baseQuery, bool isStoredProcedure)
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

            CreateSqlCommand(baseQuery, isStoredProcedure);
            AddParametersToSqlCommand(parameters);
            try
            {
                await using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    SqlCommand!.Connection = sqlConnection;
                    await sqlConnection.OpenAsync().ConfigureAwait(false);

                    return await SqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
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
        async Task GetDataFromDatabaseAsync()
        {
            SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            SqlConnection = sqlConnection;

            SqlCommand!.Connection = sqlConnection;
            await sqlConnection.OpenAsync().ConfigureAwait(false);
            DataTableCreator!.SqlDataReader = await SqlCommand!.ExecuteReaderAsync().ConfigureAwait(false);
        }

		/// <summary>
		/// Initializes component
		/// <param name="returnedTableNames">Returned table names</param>
		/// </summary>
		void InitializeComponent(List<string>? returnedTableNames)
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

        /// <summary>
        /// Saves changeset to SQL Server.
        /// </summary>
        /// <param name="changeset">Changeset to save</param>
        /// <param name="options">Save changes options</param>
        /// <param name="connectionString">Optional connection string override</param>
        /// <returns>Total affected rows</returns>
        async Task<int> SaveChangesInternalAsync(IDataSet changeset, string? connectionString)
        {
            // No-op for empty changeset (no DB work, no ConnectionString required).
            if (changeset.Tables == null || changeset.Tables.Count == 0)
            {
                return 0;
            }

            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }

            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new InvalidOperationException("ConnectionString is not specified.");
            }

            // Enforce referential integrity rules (Restrict) before any database work.
            // Changesets are often sparse (PATCH payloads), so we do NOT run orphan checks here:
            // the referenced parent rows may legitimately exist in the database but not be included in the changeset.
            // We still validate relation definitions and delete-restrict within the provided changeset.
            RelationValidationOptions relationValidationOptions = new RelationValidationOptions();
            relationValidationOptions.EnforceOrphanChecks = false;
            relationValidationOptions.EnforceDeleteRestrict = true;
            relationValidationOptions.ReportInvalidRelationDefinitions = true;
            relationValidationOptions.TreatNullForeignKeysAsNotSet = true;

            changeset.EnsureRelationsValid(relationValidationOptions);

            List<IDataTable> tablesWithChanges = ChangesetProcessor.GetTablesWithChanges(changeset);
            if (tablesWithChanges.Count == 0)
            {
                return 0;
            }

            HashSet<string> namesOfTablesWithChanges = ChangesetProcessor.GetNamesOfTablesWithChanges(tablesWithChanges);

            int affectedRows = 0;

            await using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                await sqlConnection.OpenAsync().ConfigureAwait(false);

                await using (SqlTransaction transaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        Dictionary<string, TableWriteMetadata> metadataCache = new Dictionary<string, TableWriteMetadata>(StringComparer.OrdinalIgnoreCase);

                        List<ForeignKeyEdge> edges = await MetadataLoader.LoadForeignKeyEdgesAsync(namesOfTablesWithChanges, sqlConnection, transaction).ConfigureAwait(false);
                        tablesWithChanges = TableSorter.BuildOrderedTablesWithChangesByForeignKeys(changeset, tablesWithChanges, namesOfTablesWithChanges, edges);

                        for (int t = 0; t < tablesWithChanges.Count; t++)
                        {
                            IDataTable table = tablesWithChanges[t];
                            DataTableValidator.ValidateTableForSave(table);

                            // Load and validate SQL Server schema once per table
                            TableWriteMetadata metadata = await MetadataLoader.GetOrLoadTableWriteMetadataAsync(metadataCache, table.TableName, sqlConnection, transaction).ConfigureAwait(false);
                            DataTableValidator.ValidateTableExistsInSqlServer(metadata, table.TableName);
                        }

                        affectedRows += await CommandApplier.ApplyDeletesAsync(tablesWithChanges, metadataCache, sqlConnection, transaction).ConfigureAwait(false);
                        affectedRows += await CommandApplier.ApplyInsertsAsync(tablesWithChanges, metadataCache, sqlConnection, transaction).ConfigureAwait(false);
                        affectedRows += await CommandApplier.ApplyUpdatesAsync(tablesWithChanges, metadataCache, sqlConnection, transaction).ConfigureAwait(false);

                        await transaction.CommitAsync().ConfigureAwait(false);

                        return affectedRows;
                    }
                    catch
                    {
                        try
                        {
                            await transaction.RollbackAsync().ConfigureAwait(false);
                        }
                        catch
                        {
                            // ignore rollback failures; original exception is more important
                        }

                        throw;
                    }
                    finally
                    {
                        await DisposeAsync().ConfigureAwait(false);
                    }
                }
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
        SqlCommand? SqlCommand
        {
            get; set;
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
