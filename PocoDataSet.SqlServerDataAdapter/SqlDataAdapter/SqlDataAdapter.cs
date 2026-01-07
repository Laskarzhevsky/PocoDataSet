using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

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
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Deletes data
        /// </summary>
        /// <param name="baseQuery">Base query</param>
        /// <param name="isStoredProcedure">Flag indicating whether base query is stored procedure</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Execution result</returns>
        public async Task<int> DeleteAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            return await ExecuteNonQueryAsync(baseQuery, isStoredProcedure, parameters, connectionString);
        }

        /// <summary>
        /// Fills data set
        /// </summary>
        /// <param name="baseQuery">Base query</param>
        /// <param name="isStoredProcedure">Flag indicating whether base query is stored procedure</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="returnedTableNames">Returned table names</param>
        /// <param name="connectionString">Connection string</param>
        /// <param name="dataSet">Data set</param>
        /// <returns>Data set</returns>
        public async Task<PocoDataSet.IData.IDataSet> FillAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, List<string>? returnedTableNames, string? connectionString, PocoDataSet.IData.IDataSet? dataSet)
        {
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
                await GetDataFromDatabaseAsync();
                await DataTableCreator.AddTablesToDataSetAsync();
                dataSet = DataTableCreator.DataSet;
                dataSet!.AcceptChanges();
            }
            catch (Exception exception) 
            {
                string errorMessage = exception.Message;
                throw;
            }
            finally
            {
                await DisposeAsync();
            }

            return dataSet!;
        }

        /// <summary>
        /// Inserts data
        /// </summary>
        /// <param name="baseQuery">Base query</param>
        /// <param name="isStoredProcedure">Flag indicating whether base query is stored procedure</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Execution result</returns>
        public async Task<int> InsertAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            return await ExecuteNonQueryAsync(baseQuery, isStoredProcedure, parameters, connectionString);
        }

        /// <summary>
        /// Updates data
        /// </summary>
        /// <param name="baseQuery">Base query</param>
        /// <param name="isStoredProcedure">Flag indicating whether base query is stored procedure</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Execution result</returns>
        public async Task<int> UpdateAsync(string baseQuery, bool isStoredProcedure, Dictionary<string, object?>? parameters, string? connectionString)
        {
            return await ExecuteNonQueryAsync(baseQuery, isStoredProcedure, parameters, connectionString);
        }

        /// <summary>
        /// Saves data set changes to SQL Server. The only data from rows in Added / Modified / Deleted are saved. Rows in Unchanged / Detached state are ignored.
        /// </summary>
        /// <param name="changeset">Changeset received from UI typically created by CreateChangeset method call</param>
        /// <param name="connectionString">Optional connection string to override one supplied into constructor</param>
        /// <returns>Total affected rows</returns>
        public async Task<int> SaveChangesAsync(IDataSet changeset, string? connectionString = null)
		{
			return await SaveChangesInternalAsync(changeset, connectionString);
		}

        /// <summary>
        /// Convenience method for the common UI pattern which combines SaveChangesAsync and MergeWith calls into the chain.
        /// This method intentionally does not catch exceptions — it doesn't need to.
        /// If SaveChangesAsync throws, the MergeWith call will never be done, and the caller can catch where appropriate.
        /// </summary>
        /// <param name="changeset">Changeset received from UI typically created by CreateChangeset method call</param>
        /// <param name="connectionString">Optional connection string to override one supplied into constructor</param>
        /// <returns>Total affected rows</returns>
        public async Task<int> SaveChangesAndMergePostSaveAsync(IDataSet changeset, string? connectionString = null)
        {
            int affectedRows = await SaveChangesAsync(changeset, connectionString);
            changeset.MergeWith(changeset, MergeMode.PostSave);

            return affectedRows;
        }
        #endregion
    }
}
