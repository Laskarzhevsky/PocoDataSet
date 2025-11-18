using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.Data;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides data table creator functionality
    /// </summary>
    public partial class DataTableCreator
    {
        #region events
        /// <summary>
        /// Load data table keys information equest
        /// </summary>
        internal event AsyncEventHandler<EventArgs>? LoadDataTableKeysInformationRequest;
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds tables to DataSet
        /// </summary>
        public async Task AddTablesToDataSetAsync()
        {
            VerifyDataSetExistense();
            if (ListOfTableNames == null)
            {
                await GetListOfTableNamesFromSqlDataReaderAsync();
            }
            else
            {
                CreateNewDataTable();
                await LoadDataTableKeysInformationAsync();
                AddColumnsToDataTable();
                AddPrimaryKeys();
                await AddRowsToDataTableAsync();
                AddDataTableToDataSet();
            }

            while (SqlDataReader!.NextResult())
            {
                CreateNewDataTable();
                await LoadDataTableKeysInformationAsync();
                AddColumnsToDataTable();
                AddPrimaryKeys();
                await AddRowsToDataTableAsync();
                AddDataTableToDataSet();
            }

            ReleaseResources();
        }
        #endregion


        #region Public Properties
        /// <summary>
        /// Gets or sets data set
        /// </summary>
        public PocoDataSet.IData.IDataSet? DataSet
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets data table
        /// </summary>
        public PocoDataSet.Data.DataTable? DataTable
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets foreign kes data
        /// </summary>
        public Dictionary<string, ForeignKeyData>? ForeignKeysData
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets list of table names
        /// </summary>
        public List<string>? ListOfTableNames
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets primary keys data
        /// </summary>
        public HashSet<string>? PrimaryKeyData
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets SQL data reader
        /// </summary>
        public SqlDataReader? SqlDataReader
        {
            get; set;
        }
        #endregion
    }
}
