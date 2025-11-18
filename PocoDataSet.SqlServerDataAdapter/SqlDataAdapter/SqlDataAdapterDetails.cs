using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.Data;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides SQL data adapter functionality
    /// </summary>
    public partial class SqlDataAdapter
    {
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
                SqlCommand.CommandType = CommandType.StoredProcedure;
            }
            else
            {
                SqlCommand.CommandType = CommandType.Text;
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
                SqlConnection = new SqlConnection(ConnectionString);
                SqlCommand!.Connection = SqlConnection;
                await SqlConnection.OpenAsync();

                return await SqlCommand.ExecuteNonQueryAsync();
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
            SqlConnection = new SqlConnection(ConnectionString);
            SqlCommand!.Connection = SqlConnection;
            await SqlConnection.OpenAsync();
            DataTableCreator!.SqlDataReader = await SqlCommand!.ExecuteReaderAsync();
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
        /// Loads data table foreign keys
        /// </summary>
        async Task LoadDataTableForeignKeysAsync()
        {
            Dictionary<string, ForeignKeyData> foreignKeysData = new Dictionary<string, ForeignKeyData>();

            string sqlStatement = @"
                SELECT 
                    fk.name AS ForeignKeyName,
                    parent.name AS ParentTable,
                    pc.name AS ParentColumn,
                    referenced.name AS ReferencedTable,
                    rc.name AS ReferencedColumn
                FROM 
                    sys.foreign_keys fk
                JOIN 
                    sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                JOIN 
                    sys.tables parent ON parent.object_id = fk.parent_object_id
                JOIN 
                    sys.columns pc ON pc.object_id = parent.object_id AND pc.column_id = fkc.parent_column_id
                JOIN 
                    sys.tables referenced ON referenced.object_id = fk.referenced_object_id
                JOIN 
                    sys.columns rc ON rc.object_id = referenced.object_id AND rc.column_id = fkc.referenced_column_id
                WHERE 
                    parent.name = @tableName;";

            using SqlCommand sqlCommand = new SqlCommand(sqlStatement, SqlConnection);
            sqlCommand.Parameters.AddWithValue("@tableName", DataTableCreator!.DataTable!.TableName);

            using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
            while (await sqlDataReader.ReadAsync())
            {
                ForeignKeyData foreignKeyData = new ForeignKeyData();

                foreignKeyData.ForeignKeyName = sqlDataReader.GetString(0);
                foreignKeyData.ParentTableName = sqlDataReader.GetString(1);
                foreignKeyData.ParentColumnName = sqlDataReader.GetString(2);
                foreignKeyData.ReferencedTableName = sqlDataReader.GetString(3);
                foreignKeyData.ReferencedColumnName = sqlDataReader.GetString(4);

                foreignKeysData[foreignKeyData.ParentColumnName] = foreignKeyData;
            }

            DataTableCreator.ForeignKeysData = foreignKeysData;
        }

        /// <summary>
        /// Loads data table Primary keys
        /// </summary>
        async Task LoadDataTablePrimaryKeysAsync()
        {
            HashSet<string> primaryKeysData = new HashSet<string>();
            var sqlStatement = @"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE TABLE_NAME = @tableName
                AND CONSTRAINT_NAME IN (
                    SELECT CONSTRAINT_NAME
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                    WHERE TABLE_NAME = @tableName
                    AND CONSTRAINT_TYPE = 'PRIMARY KEY'
                )";

            using SqlCommand sqlCommand = new SqlCommand(sqlStatement, SqlConnection);
            sqlCommand.Parameters.AddWithValue("@tableName", DataTableCreator!.DataTable!.TableName);

            using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
            while (await sqlDataReader.ReadAsync())
            {
                var columnName = sqlDataReader.GetString(0);
                primaryKeysData.Add(columnName);
            }

            DataTableCreator.PrimaryKeyData = primaryKeysData;
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

            if (SqlCommand is not null)
            {
                SqlCommand.Connection = null;
                await SqlCommand.DisposeAsync().ConfigureAwait(false);
                SqlCommand = null;
            }

            if (SqlConnection is not null)
            {
                await SqlConnection.DisposeAsync().ConfigureAwait(false);
                SqlConnection = null;
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

        /// <summary>
        /// Gets or sets SQL connection
        /// </summary>
        SqlConnection? SqlConnection
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
            await LoadDataTablePrimaryKeysAsync();
            await LoadDataTableForeignKeysAsync();
        }
        #endregion
    }
}
