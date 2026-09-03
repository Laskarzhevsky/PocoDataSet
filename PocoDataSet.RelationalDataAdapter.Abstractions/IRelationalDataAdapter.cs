using System.Collections.Generic;
using System.Threading.Tasks;

using PocoDataSet.IData;

namespace PocoDataSet.RelationalDataAdapter.Abstractions
{
    /// <summary>
    /// Defines the provider-independent surface of a relational POCO DataSet adapter.
    /// Provider-specific features, such as SQL Server table-valued parameters or
    /// PostgreSQL array/jsonb parameters, should remain outside this interface.
    /// </summary>
    public interface IRelationalDataAdapter
    {
        #region Methods
        /// <summary>
        /// Fills a data set from a query or stored procedure.
        /// </summary>
        Task<IDataSet> FillAsync(
            string baseQuery,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters,
            List<string>? returnedTableNames,
            string? connectionString,
            IDataSet? dataSet);

        /// <summary>
        /// Fills an existing data set from a query or stored procedure.
        /// </summary>
        Task<IDataSet> FillIntoExistingDataSetAsync(
            IDataSet dataSet,
            string baseQuery,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters,
            List<string>? returnedTableNames,
            string? connectionString);

        /// <summary>
        /// Executes a query or stored procedure that does not return rows.
        /// </summary>
        Task<int> ExecuteNonQueryAsync(
            string baseQuery,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters,
            string? connectionString);

        /// <summary>
        /// Executes a query or stored procedure and returns the first column of the first row.
        /// </summary>
        Task<object?> ExecuteScalarAsync(
            string baseQuery,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters,
            string? connectionString);

        /// <summary>
        /// Inserts data through a provider-specific command or stored procedure.
        /// </summary>
        Task<int> InsertAsync(
            string baseQuery,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters,
            string? connectionString);

        /// <summary>
        /// Updates data through a provider-specific command or stored procedure.
        /// </summary>
        Task<int> UpdateAsync(
            string baseQuery,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters,
            string? connectionString);

        /// <summary>
        /// Deletes data through a provider-specific command or stored procedure.
        /// </summary>
        Task<int> DeleteAsync(
            string baseQuery,
            bool isStoredProcedure,
            Dictionary<string, object?>? parameters,
            string? connectionString);

        /// <summary>
        /// Saves changed rows from a POCO DataSet using the provider implementation.
        /// </summary>
        Task<int> SaveChangesAsync(IDataSet changeset, string? connectionString = null);
        #endregion
    }
}
