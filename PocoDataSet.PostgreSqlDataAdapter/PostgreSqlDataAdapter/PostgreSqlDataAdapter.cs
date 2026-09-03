using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.RelationalDataAdapter.Abstractions;

namespace PocoDataSet.PostgreSqlDataAdapter
{
    /// <summary>
    /// Provides PostgreSQL data adapter functionality through Npgsql.
    /// </summary>
    public partial class PostgreSqlDataAdapter : AsyncDisposableObject, IRelationalDataAdapter
    {
        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        public PostgreSqlDataAdapter(string? connectionString)
        {
            ConnectionString = connectionString;
            OwnsConnection = true;
        }

        /// <summary>
        /// Creates adapter that uses an already opened PostgreSQL connection.
        /// The adapter will not dispose this connection.
        /// </summary>
        /// <param name="connection">Existing PostgreSQL connection.</param>
        public PostgreSqlDataAdapter(NpgsqlConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            Connection = connection;
            ConnectionString = connection.ConnectionString;
            OwnsConnection = false;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets connection string.
        /// </summary>
        public string? ConnectionString
        {
            get; set;
        }
        #endregion

        #region Internal Properties
        internal NpgsqlConnection? Connection
        {
            get; set;
        }

        internal bool OwnsConnection
        {
            get; private set;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Deletes data.
        /// </summary>
        public async Task<int> DeleteAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            return await ExecuteNonQueryAsync(baseQuery, isStoredProcedure, parameters, connectionString).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query or stored procedure that does not return rows.
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            int affectedRows;
            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }

            try
            {
                NpgsqlConnection connection = GetOrCreateConnection();
                await OpenConnectionAsync(connection).ConfigureAwait(false);

                await using (NpgsqlCommand command = CreateCommand(baseQuery, isStoredProcedure, connection))
                {
                    AddParameters(command, parameters);
                    affectedRows = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await DisposeAsync().ConfigureAwait(false);
            }

            return affectedRows;
        }

        /// <summary>
        /// Executes a query or stored procedure and returns the first column of the first row.
        /// </summary>
        public async Task<object?> ExecuteScalarAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            object? result;
            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }

            try
            {
                NpgsqlConnection connection = GetOrCreateConnection();
                await OpenConnectionAsync(connection).ConfigureAwait(false);

                await using (NpgsqlCommand command = CreateCommand(baseQuery, isStoredProcedure, connection))
                {
                    AddParameters(command, parameters);
                    result = await command.ExecuteScalarAsync().ConfigureAwait(false);
                    if (result == DBNull.Value)
                    {
                        result = null;
                    }
                }
            }
            finally
            {
                await DisposeAsync().ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Fills a data set from a query or stored procedure.
        /// </summary>
        public async Task<IDataSet> FillAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, List<string>? returnedTableNames, string? connectionString, IDataSet? dataSet)
        {
            IDataSet result;
            if (dataSet == null)
            {
                dataSet = new PocoDataSet.Data.DataSet();
            }

            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }

            try
            {
                NpgsqlConnection connection = GetOrCreateConnection();
                await OpenConnectionAsync(connection).ConfigureAwait(false);

                await using (NpgsqlCommand command = CreateCommand(baseQuery, isStoredProcedure, connection))
                {
                    AddParameters(command, parameters);
                    await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        PostgreSqlDataTableCreator dataTableCreator = new PostgreSqlDataTableCreator();
                        dataTableCreator.DataSet = dataSet;
                        dataTableCreator.ListOfTableNames = returnedTableNames;
                        dataTableCreator.DataReader = reader;
                        await dataTableCreator.AddTablesToDataSetAsync().ConfigureAwait(false);
                    }
                }

                dataSet.AcceptChanges();
                result = dataSet;
            }
            finally
            {
                await DisposeAsync().ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Fills an existing data set from a query or stored procedure.
        /// </summary>
        public async Task<IDataSet> FillIntoExistingDataSetAsync(IDataSet dataSet, string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, List<string>? returnedTableNames, string? connectionString)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException(nameof(dataSet));
            }

            return await FillAsync(baseQuery, isStoredProcedure, parameters, returnedTableNames, connectionString, dataSet).ConfigureAwait(false);
        }

        /// <summary>
        /// Fills a data set using provider-specific PostgreSQL parameters.
        /// </summary>
        public async Task<IDataSet> FillAsync(string baseQuery, bool isStoredProcedure, params NpgsqlParameter[] parameters)
        {
            return await FillAsync(baseQuery, isStoredProcedure, parameters, null, null, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Fills a data set using provider-specific PostgreSQL parameters.
        /// </summary>
        public async Task<IDataSet> FillAsync(string baseQuery, bool isStoredProcedure, NpgsqlParameter[]? parameters, List<string>? returnedTableNames, string? connectionString, IDataSet? dataSet)
        {
            IDataSet result;
            if (dataSet == null)
            {
                dataSet = new PocoDataSet.Data.DataSet();
            }

            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }

            try
            {
                NpgsqlConnection connection = GetOrCreateConnection();
                await OpenConnectionAsync(connection).ConfigureAwait(false);

                await using (NpgsqlCommand command = CreateCommand(baseQuery, isStoredProcedure, connection))
                {
                    AddParameters(command, parameters);
                    await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        PostgreSqlDataTableCreator dataTableCreator = new PostgreSqlDataTableCreator();
                        dataTableCreator.DataSet = dataSet;
                        dataTableCreator.ListOfTableNames = returnedTableNames;
                        dataTableCreator.DataReader = reader;
                        await dataTableCreator.AddTablesToDataSetAsync().ConfigureAwait(false);
                    }
                }

                dataSet.AcceptChanges();
                result = dataSet;
            }
            finally
            {
                await DisposeAsync().ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Creates a PostgreSQL array parameter. This is the PostgreSQL alternative for many list-passing scenarios.
        /// </summary>
        public NpgsqlParameter CreateArrayParameter<T>(string parameterName, NpgsqlDbType elementType, IEnumerable<T> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            NpgsqlParameter parameter = new NpgsqlParameter();
            parameter.ParameterName = parameterName;
            parameter.NpgsqlDbType = NpgsqlDbType.Array | elementType;
            parameter.Value = values;
            return parameter;
        }

        /// <summary>
        /// Creates a PostgreSQL jsonb parameter.
        /// </summary>
        public NpgsqlParameter CreateJsonbParameter(string parameterName, object? value)
        {
            NpgsqlParameter parameter = new NpgsqlParameter();
            parameter.ParameterName = parameterName;
            parameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = value;
            }

            return parameter;
        }

        /// <summary>
        /// Inserts data.
        /// </summary>
        public async Task<int> InsertAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            return await ExecuteNonQueryAsync(baseQuery, isStoredProcedure, parameters, connectionString).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves changed rows from a POCO DataSet.
        /// </summary>
        public Task<int> SaveChangesAsync(IDataSet changeset, string? connectionString = null)
        {
            throw new NotSupportedException("PostgreSqlDataAdapter does not yet provide generated SaveChanges persistence. Use InsertAsync, UpdateAsync, DeleteAsync, ExecuteNonQueryAsync, or provider-specific PostgreSQL commands until PostgreSQL save command generation is implemented.");
        }

        /// <summary>
        /// Updates data.
        /// </summary>
        public async Task<int> UpdateAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            return await ExecuteNonQueryAsync(baseQuery, isStoredProcedure, parameters, connectionString).ConfigureAwait(false);
        }
        #endregion

        #region Protected Methods
        protected override void ReleaseResources()
        {
            if (OwnsConnection && Connection != null)
            {
                Connection.Dispose();
            }

            Connection = null;
        }

        protected override async ValueTask ReleaseResourcesAsync()
        {
            if (OwnsConnection && Connection != null)
            {
                await Connection.DisposeAsync().ConfigureAwait(false);
            }

            Connection = null;
        }
        #endregion

        #region Internal Methods
        internal void AddParameters(NpgsqlCommand command, Dictionary<string, object?>? parameters)
        {
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object?> parameter in parameters)
                {
                    NpgsqlParameter npgsqlParameter = command.CreateParameter();
                    npgsqlParameter.ParameterName = parameter.Key;
                    if (parameter.Value == null)
                    {
                        npgsqlParameter.Value = DBNull.Value;
                    }
                    else
                    {
                        npgsqlParameter.Value = parameter.Value;
                    }

                    command.Parameters.Add(npgsqlParameter);
                }
            }
        }

        internal void AddParameters(NpgsqlCommand command, NpgsqlParameter[]? parameters)
        {
            if (parameters == null)
            {
                return;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                NpgsqlParameter parameter = parameters[i];
                if (parameter == null)
                {
                    continue;
                }

                command.Parameters.Add(parameter);
            }
        }

        internal NpgsqlCommand CreateCommand(string baseQuery, bool isStoredProcedure, NpgsqlConnection connection)
        {
            NpgsqlCommand command = new NpgsqlCommand();
            command.CommandText = baseQuery;
            command.Connection = connection;
            if (isStoredProcedure)
            {
                command.CommandType = CommandType.StoredProcedure;
            }
            else
            {
                command.CommandType = CommandType.Text;
            }

            return command;
        }

        internal NpgsqlConnection GetOrCreateConnection()
        {
            NpgsqlConnection? connection = Connection;
            if (connection == null)
            {
                if (string.IsNullOrEmpty(ConnectionString))
                {
                    throw new InvalidOperationException("ConnectionString is not set.");
                }

                connection = new NpgsqlConnection(ConnectionString);
                Connection = connection;
                OwnsConnection = true;
            }

            return connection;
        }

        internal async Task OpenConnectionAsync(NpgsqlConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
        }
        #endregion
    }
}
