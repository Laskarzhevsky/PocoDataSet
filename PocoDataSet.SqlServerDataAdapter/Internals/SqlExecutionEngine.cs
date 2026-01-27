using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.IData;
using PocoDataSet.Extensions;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Centralizes SQL execution behavior for <see cref="SqlDataAdapter"/> and <see cref="SqlDataAdapterTransaction"/>.
    /// This class contains the "how to execute" logic; connection/transaction lifetime is owned by the caller.
    /// </summary>
    internal sealed class SqlExecutionEngine
    {
        #region Private Fields
        readonly SqlDataAdapter _adapter;
        #endregion

        #region Constructors
        public SqlExecutionEngine(SqlDataAdapter adapter)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }
        #endregion

        #region Public Methods
        public async Task<int> ExecuteNonQueryAsync(
            string commandText,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters,
            SqlConnection sqlConnection,
            SqlTransaction? sqlTransaction)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text is not specified.", nameof(commandText));
            }

            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            SqlCommand? priorCommand = _adapter.SqlCommand;
            SqlCommand? createdCommand = null;

            try
            {
                _adapter.CreateSqlCommand(commandText, isStoredProcedure);
                _adapter.AddParametersToSqlCommand(parameters);

                createdCommand = _adapter.SqlCommand!;
                createdCommand.Connection = sqlConnection;
                createdCommand.Transaction = sqlTransaction;

                if (sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await sqlConnection.OpenAsync().ConfigureAwait(false);
                }

                return await createdCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            finally
            {
                _adapter.SqlCommand = priorCommand;

                if (createdCommand != null)
                {
                    createdCommand.Dispose();
                }
            }
        }

        public async Task<object?> ExecuteScalarAsync(
            string commandText,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters,
            SqlConnection sqlConnection,
            SqlTransaction? sqlTransaction)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text is not specified.", nameof(commandText));
            }

            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            SqlCommand? priorCommand = _adapter.SqlCommand;
            SqlCommand? createdCommand = null;

            try
            {
                _adapter.CreateSqlCommand(commandText, isStoredProcedure);
                _adapter.AddParametersToSqlCommand(parameters);

                createdCommand = _adapter.SqlCommand!;
                createdCommand.Connection = sqlConnection;
                createdCommand.Transaction = sqlTransaction;

                if (sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await sqlConnection.OpenAsync().ConfigureAwait(false);
                }

                return await createdCommand.ExecuteScalarAsync().ConfigureAwait(false);
            }
            finally
            {
                _adapter.SqlCommand = priorCommand;

                if (createdCommand != null)
                {
                    createdCommand.Dispose();
                }
            }
        }

        public async Task<IDataSet> ExecuteQueryToDataSetAsync(
            string baseQuery,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters,
            List<string>? returnedTableNames,
            IDataSet? dataSet,
            SqlConnection sqlConnection,
            SqlTransaction? sqlTransaction)
        {
            if (string.IsNullOrEmpty(baseQuery))
            {
                throw new ArgumentException("Base query is not specified.", nameof(baseQuery));
            }

            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            IDataSet resultDataSet = dataSet ?? DataSetFactory.CreateDataSet();

            DataTableCreator? priorCreator = _adapter.DataTableCreator;
            SqlCommand? priorCommand = _adapter.SqlCommand;
            SqlConnection? priorConnection = _adapter.SqlConnection;
            SqlTransaction? priorTransaction = _adapter.SqlTransaction;

            try
            {
                _adapter.InitializeComponent(returnedTableNames);
                _adapter.DataTableCreator!.DataSet = resultDataSet;

                _adapter.CreateSqlCommand(baseQuery, isStoredProcedure);
                _adapter.AddParametersToSqlCommand(parameters);

                await _adapter.GetDataFromDatabaseAsync(sqlConnection, sqlTransaction).ConfigureAwait(false);
                await _adapter.DataTableCreator.AddTablesToDataSetAsync().ConfigureAwait(false);

                resultDataSet = _adapter.DataTableCreator.DataSet!;
                resultDataSet.AcceptChanges();

                return resultDataSet;
            }
            finally
            {
                // Dispose per-query resources, but do NOT dispose the shared connection/transaction.
                if (_adapter.DataTableCreator != null && _adapter.DataTableCreator.SqlDataReader != null)
                {
                    await _adapter.DataTableCreator.SqlDataReader.DisposeAsync().ConfigureAwait(false);
                    _adapter.DataTableCreator.SqlDataReader = null;
                }

                _adapter.DataTableCreator = priorCreator;
                _adapter.SqlCommand = priorCommand;
                _adapter.SqlConnection = priorConnection;
                _adapter.SqlTransaction = priorTransaction;
            }
        }

        public async Task<int> SaveChangesAsync(IDataSet changeset, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
        {
            if (changeset == null)
            {
                throw new ArgumentNullException(nameof(changeset));
            }

            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (sqlTransaction == null)
            {
                throw new ArgumentNullException(nameof(sqlTransaction));
            }

            SqlConnection? priorConnection = _adapter.SqlConnection;
            SqlTransaction? priorTransaction = _adapter.SqlTransaction;

            try
            {
                _adapter.SqlConnection = sqlConnection;
                _adapter.SqlTransaction = sqlTransaction;

                SaveChangesDataPersistenceLogicHandler handler = new SaveChangesDataPersistenceLogicHandler(_adapter);
                return await handler.SaveChangesAsync(changeset).ConfigureAwait(false);
            }
            finally
            {
                _adapter.SqlConnection = priorConnection;
                _adapter.SqlTransaction = priorTransaction;
            }
        }
        #endregion
    }
}
