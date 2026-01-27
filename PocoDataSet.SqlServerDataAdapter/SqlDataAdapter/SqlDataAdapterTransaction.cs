using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.Extensions.Relations;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Represents a SQL Server transaction scope for <see cref="SqlDataAdapter"/>.
    /// </summary>
    public sealed class SqlDataAdapterTransaction : AsyncDisposableObject
    {
        #region Private Fields
        readonly SqlDataAdapter _adapter;
        readonly SqlConnection _sqlConnection;
        readonly SqlTransaction _sqlTransaction;
        readonly bool _ownsConnection;

        bool _isCompleted;
        #endregion

        #region Constructors
        internal SqlDataAdapterTransaction(
            SqlDataAdapter adapter,
            SqlConnection sqlConnection,
            SqlTransaction sqlTransaction,
            bool ownsConnection)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _sqlConnection = sqlConnection ?? throw new ArgumentNullException(nameof(sqlConnection));
            _sqlTransaction = sqlTransaction ?? throw new ArgumentNullException(nameof(sqlTransaction));
            _ownsConnection = ownsConnection;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes a non-query SQL command within the current transaction.
        /// </summary>
        public Task<int> ExecuteNonQueryAsync(
            string commandText,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters = null)
        {
            return _adapter.ExecutionEngine.ExecuteNonQueryAsync(
                commandText,
                isStoredProcedure,
                parameters,
                _sqlConnection,
                _sqlTransaction);
        }

        /// <summary>
        /// Executes a scalar SQL command within the current transaction.
        /// </summary>
        public Task<object?> ExecuteScalarAsync(
            string commandText,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters = null)
        {
            return _adapter.ExecutionEngine.ExecuteScalarAsync(
                commandText,
                isStoredProcedure,
                parameters,
                _sqlConnection,
                _sqlTransaction);
        }

        /// <summary>
        /// Executes a query and materializes results into an <see cref="IDataSet"/> within the current transaction.
        /// </summary>
        public Task<IDataSet> ExecuteQueryAsync(
            string baseQuery,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters = null,
            List<string>? returnedTableNames = null,
            IDataSet? dataSet = null)
        {
            return _adapter.ExecutionEngine.ExecuteQueryToDataSetAsync(
                baseQuery,
                isStoredProcedure,
                parameters,
                returnedTableNames,
                dataSet,
                _sqlConnection,
                _sqlTransaction);
        }

        /// <summary>
        /// Saves the provided changeset within the current transaction.
        /// </summary>
        public async Task<int> SaveChangesAsync(IDataSet changeset)
        {
            if (changeset == null)
            {
                throw new ArgumentNullException(nameof(changeset));
            }

            // No-op for empty changeset (no DB work).
            if (changeset.Tables == null || changeset.Tables.Count == 0)
            {
                return 0;
            }

            RelationValidationOptions relationValidationOptions = new RelationValidationOptions();
            relationValidationOptions.EnforceDeleteRestrict = true;
            relationValidationOptions.ReportInvalidRelationDefinitions = true;
            relationValidationOptions.TreatNullForeignKeysAsNotSet = true;

            changeset.EnsureRelationsValid(relationValidationOptions);

            return await _adapter.ExecutionEngine
                .SaveChangesAsync(changeset, _sqlConnection, _sqlTransaction)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        public Task CommitAsync()
        {
            if (_isCompleted)
            {
                return Task.CompletedTask;
            }

            _sqlTransaction.Commit();
            _isCompleted = true;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        public Task RollbackAsync()
        {
            if (_isCompleted)
            {
                return Task.CompletedTask;
            }

            _sqlTransaction.Rollback();
            _isCompleted = true;

            return Task.CompletedTask;
        }
        #endregion

        #region AsyncDisposableObject Overrides
        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncCore()
        {
            try
            {
                if (!_isCompleted)
                {
                    try
                    {
                        _sqlTransaction.Rollback();
                    }
                    catch
                    {
                        // swallow rollback errors on dispose
                    }

                    _isCompleted = true;
                }
            }
            finally
            {
                _sqlTransaction.Dispose();

                if (_ownsConnection)
                {
                    await _sqlConnection.DisposeAsync().ConfigureAwait(false);
                }
            }
        }
        #endregion
    }
}
