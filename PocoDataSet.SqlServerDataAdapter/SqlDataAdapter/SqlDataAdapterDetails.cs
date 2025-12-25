using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides SQL data adapter functionality
    /// </summary>
    public partial class SqlDataAdapter
    {
        #region Methods
        /// <summary>
        /// Saves changeset to SQL Server.
        /// </summary>
        /// <param name="changeset">Changeset to save</param>
        /// <param name="options">Save changes options</param>
        /// <param name="connectionString">Optional connection string override</param>
        /// <returns>Total affected rows</returns>
        async Task<int> SaveChangesInternalAsync(IDataSet changeset, SqlServerSaveChangesOptions? options, string? connectionString)
		{
			if (options == null)
			{
				options = new SqlServerSaveChangesOptions();
			}

			if (!string.IsNullOrEmpty(connectionString))
			{
				ConnectionString = connectionString;
			}

			if (string.IsNullOrEmpty(ConnectionString))
			{
				throw new InvalidOperationException("ConnectionString is not specified.");
			}

			int affectedRows = 0;

			SqlConnection = new SqlConnection(ConnectionString);
			await SqlConnection.OpenAsync().ConfigureAwait(false);

			SqlTransaction? transaction = null;
			if (options.UseTransaction)
			{
				transaction = SqlConnection.BeginTransaction();
			}

			try
			{
                Dictionary<string, TableWriteMetadata> metadataCache = new Dictionary<string, TableWriteMetadata>(StringComparer.OrdinalIgnoreCase);
                List<IDataTable> orderedTables = BuildOrderedTables(changeset, options);
                for (int i = 0; i < orderedTables.Count; i++)
                {
                    IDataTable table = orderedTables[i];
                    ValidateTableForSave(table);
                    await GetOrLoadTableWriteMetadataAsync(metadataCache, table.TableName, transaction).ConfigureAwait(false);
                }
                
				affectedRows += await ApplyDeletesAsync(orderedTables, metadataCache, options, transaction).ConfigureAwait(false);
				affectedRows += await ApplyInsertsAsync(orderedTables, metadataCache, options, transaction).ConfigureAwait(false);
				affectedRows += await ApplyUpdatesAsync(orderedTables, metadataCache, options, transaction).ConfigureAwait(false);

				if (transaction != null)
				{
					transaction.Commit();
				}
			}
			catch
			{
				if (transaction != null)
				{
					try
					{
						transaction.Rollback();
					}
					catch
					{
						// ignore rollback exceptions
					}
				}
				throw;
			}
			finally
			{
				if (transaction != null)
				{
					await transaction.DisposeAsync().ConfigureAwait(false);
				}
				await DisposeAsync().ConfigureAwait(false);
			}

			return affectedRows;
		}

		/// <summary>
		/// Builds ordered list of tables based on options.TableSaveOrder.
		/// </summary>
		List<IDataTable> BuildOrderedTables(IDataSet changeset, SqlServerSaveChangesOptions options)
		{
			List<IDataTable> orderedTables = new List<IDataTable>();
			HashSet<string> alreadyAdded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			if (options.TableSaveOrder != null && options.TableSaveOrder.Count > 0)
			{
				for (int i = 0; i < options.TableSaveOrder.Count; i++)
				{
					string tableName = options.TableSaveOrder[i];
					if (string.IsNullOrEmpty(tableName))
					{
						continue;
					}

					IDataTable? dataTable;
					changeset.Tables.TryGetValue(tableName, out dataTable);
					if (dataTable != null)
					{
						orderedTables.Add(dataTable);
						alreadyAdded.Add(tableName);
					}
				}
			}

			foreach (KeyValuePair<string, IDataTable> kvp in changeset.Tables)
			{
				if (alreadyAdded.Contains(kvp.Key))
				{
					continue;
				}
				orderedTables.Add(kvp.Value);
			}

			return orderedTables;
		}


		sealed class TableWriteMetadata
		{
			public string TableName
			{
				get; set;
			} = string.Empty;

			public HashSet<string> ColumnNames
			{
				get;
			} = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			public List<string> PrimaryKeyColumns
			{
				get;
			} = new List<string>();

			public HashSet<string> PrimaryKeys
			{
				get;
			} = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			public HashSet<string> IdentityColumns
			{
				get;
			} = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			public HashSet<string> ComputedColumns
			{
				get;
			} = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			public HashSet<string> RowVersionColumns
			{
				get;
			} = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		async Task<TableWriteMetadata> LoadTableWriteMetadataAsync(string tableName, SqlTransaction? transaction)
		{
			if (string.IsNullOrEmpty(tableName))
			{
				throw new ArgumentException("Table name is empty.", nameof(tableName));
			}

			TableWriteMetadata metadata = new TableWriteMetadata();
			metadata.TableName = tableName;

			// 1) Columns + identity/computed/rowversion detection
			string columnsSql = @"
SELECT
	c.name AS ColumnName,
	c.is_identity AS IsIdentity,
	c.is_computed AS IsComputed,
	t.name AS TypeName,
	c.system_type_id AS SystemTypeId
FROM sys.tables tb
JOIN sys.columns c ON c.object_id = tb.object_id
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE tb.name = @tableName;";

			using (SqlCommand columnsCommand = new SqlCommand(columnsSql, SqlConnection, transaction))
			{
				columnsCommand.Parameters.AddWithValue("@tableName", tableName);

				using (SqlDataReader reader = await columnsCommand.ExecuteReaderAsync().ConfigureAwait(false))
				{
					while (await reader.ReadAsync().ConfigureAwait(false))
					{
						string columnName = reader.GetString(0);
						bool isIdentity = reader.GetBoolean(1);
						bool isComputed = reader.GetBoolean(2);
						string typeName = reader.GetString(3);
                        int systemTypeId = reader.GetByte(4);
                        
						metadata.ColumnNames.Add(columnName);

						if (isIdentity)
						{
							metadata.IdentityColumns.Add(columnName);
						}

						if (isComputed)
						{
							metadata.ComputedColumns.Add(columnName);
						}

						// SQL Server rowversion/timestamp has system_type_id = 189
						if (systemTypeId == 189 || string.Equals(typeName, "rowversion", StringComparison.OrdinalIgnoreCase) || string.Equals(typeName, "timestamp", StringComparison.OrdinalIgnoreCase))
						{
							metadata.RowVersionColumns.Add(columnName);
						}
					}
				}
			}

            if (metadata.ColumnNames.Count == 0)
            {
                throw new InvalidOperationException("Table '" + tableName + "' does not exist in database schema.");
            }

            // 2) Primary key columns
            string pkSql = @"
SELECT c.name AS ColumnName
FROM sys.indexes i
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
JOIN sys.tables t ON t.object_id = i.object_id
WHERE i.is_primary_key = 1
AND t.name = @tableName
ORDER BY ic.key_ordinal;";

			using (SqlCommand pkCommand = new SqlCommand(pkSql, SqlConnection, transaction))
			{
				pkCommand.Parameters.AddWithValue("@tableName", tableName);

				using (SqlDataReader reader = await pkCommand.ExecuteReaderAsync().ConfigureAwait(false))
				{
					while (await reader.ReadAsync().ConfigureAwait(false))
					{
						string pkColumnName = reader.GetString(0);
						metadata.PrimaryKeys.Add(pkColumnName);
						metadata.PrimaryKeyColumns.Add(pkColumnName);
					}
				}
			}

			return metadata;
		}

		async Task<TableWriteMetadata> GetOrLoadTableWriteMetadataAsync(Dictionary<string, TableWriteMetadata> cache, string tableName, SqlTransaction? transaction)
		{
			TableWriteMetadata? metadata;
			cache.TryGetValue(tableName, out metadata);
			if (metadata != null)
			{
				return metadata;
			}

			metadata = await LoadTableWriteMetadataAsync(tableName, transaction).ConfigureAwait(false);
			cache[tableName] = metadata;
			return metadata;
		}

		HashSet<string> BuildExcludedColumns(IDataTable table, TableWriteMetadata metadata, SqlServerSaveChangesOptions options)
		{
			HashSet<string> excluded = GetExcludedColumns(table.TableName, options);

			// Always exclude computed and rowversion
			foreach (string col in metadata.ComputedColumns)
			{
				excluded.Add(col);
			}
			foreach (string col in metadata.RowVersionColumns)
			{
				excluded.Add(col);
			}

			// Exclude identity on INSERT (and typically on UPDATE too)
			foreach (string col in metadata.IdentityColumns)
			{
				excluded.Add(col);
			}

			return excluded;
		}


		async Task<int> ApplyDeletesAsync(List<IDataTable> orderedTables, Dictionary<string, TableWriteMetadata> metadataCache, SqlServerSaveChangesOptions options, SqlTransaction? transaction)
		{
			int affected = 0;
			for (int t = orderedTables.Count - 1; t >= 0; t--)
			{
				IDataTable table = orderedTables[t];
				TableWriteMetadata metadata = metadataCache[table.TableName];
                for (int i = 0; i < table.Rows.Count; i++)
				{
					IDataRow row = table.Rows[i];
					if (row.DataRowState != DataRowState.Deleted)
					{
						continue;
					}

					using SqlCommand sqlCommand = BuildDeleteCommand(table, metadata, row, options, transaction);
					affected += await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
				}
			}
			return affected;
		}

		async Task<int> ApplyInsertsAsync(List<IDataTable> orderedTables, Dictionary<string, TableWriteMetadata> metadataCache, SqlServerSaveChangesOptions options, SqlTransaction? transaction)
		{
			int affected = 0;
			for (int t = 0; t < orderedTables.Count; t++)
			{
				IDataTable table = orderedTables[t];
                TableWriteMetadata metadata = metadataCache[table.TableName];
                for (int i = 0; i < table.Rows.Count; i++)
				{
					IDataRow row = table.Rows[i];
					if (row.DataRowState != DataRowState.Added)
					{
						continue;
					}

					using SqlCommand sqlCommand = BuildInsertCommand(table, metadata, row, options, transaction);
					affected += await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
				}
			}
			return affected;
		}

		async Task<int> ApplyUpdatesAsync(List<IDataTable> orderedTables, Dictionary<string, TableWriteMetadata> metadataCache, SqlServerSaveChangesOptions options, SqlTransaction? transaction)
		{
			int affected = 0;
			for (int t = 0; t < orderedTables.Count; t++)
			{
				IDataTable table = orderedTables[t];
                TableWriteMetadata metadata = metadataCache[table.TableName];
                for (int i = 0; i < table.Rows.Count; i++)
				{
					IDataRow row = table.Rows[i];
					if (row.DataRowState != DataRowState.Modified)
					{
						continue;
					}

					using SqlCommand sqlCommand = BuildUpdateCommand(table, metadata, row, options, transaction);
					if (sqlCommand.CommandText.Length == 0)
					{
						continue;
					}

					affected += await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
				}
			}

			return affected;
		}

		void ValidateTableForSave(IDataTable table)
		{
			if (string.IsNullOrEmpty(table.TableName))
			{
				throw new InvalidOperationException("TableName is not specified.");
			}

			if (table.Columns == null)
			{
				throw new InvalidOperationException("Columns collection is null.");
			}

			if (table.Rows == null)
			{
				throw new InvalidOperationException("Rows collection is null.");
			}
		}

		SqlCommand BuildInsertCommand(IDataTable table, TableWriteMetadata metadata, IDataRow row, SqlServerSaveChangesOptions options, SqlTransaction? transaction)
		{
			List<string> columnNames = new List<string>();
			List<string> parameterNames = new List<string>();
			List<SqlParameter> sqlParameters = new List<SqlParameter>();

			HashSet<string> excludedColumns = BuildExcludedColumns(table, metadata, options);
			int parameterIndex = 0;
			for (int i = 0; i < table.Columns.Count; i++)
			{
				string columnName = table.Columns[i].ColumnName;
				if (!metadata.ColumnNames.Contains(columnName))
				{
					// Column does not exist in server schema
					continue;
				}
				if (excludedColumns.Contains(columnName))
				{
					continue;
				}

				string parameterName = "@p" + parameterIndex;
				parameterIndex++;

				columnNames.Add(EscapeIdentifier(columnName));
				parameterNames.Add(parameterName);
				object? value;
				row.TryGetValue(columnName, out value);
				sqlParameters.Add(CreateSqlParameter(parameterName, value));
			}

            if (columnNames.Count == 0)
            {
                throw new InvalidOperationException("Cannot INSERT into table '" + table.TableName + "': no writable columns found.");
            }

            string sql = "INSERT INTO " + EscapeIdentifier(table.TableName) + " (" + string.Join(",", columnNames) + ") VALUES (" + string.Join(",", parameterNames) + ")";
			SqlCommand sqlCommand = new SqlCommand(sql, SqlConnection, transaction);
			for (int i = 0; i < sqlParameters.Count; i++)
			{
				sqlCommand.Parameters.Add(sqlParameters[i]);
			}

			return sqlCommand;
		}

		SqlCommand BuildUpdateCommand(IDataTable table, TableWriteMetadata metadata, IDataRow row, SqlServerSaveChangesOptions options, SqlTransaction? transaction)
		{
			EnsurePrimaryKeys(metadata, table.TableName);

			HashSet<string> excludedColumns = BuildExcludedColumns(table, metadata, options);

			List<string> setClauses = new List<string>();
			List<SqlParameter> sqlParameters = new List<SqlParameter>();
			int parameterIndex = 0;

			for (int i = 0; i < table.Columns.Count; i++)
			{
				string columnName = table.Columns[i].ColumnName;
				if (!metadata.ColumnNames.Contains(columnName))
				{
					continue;
				}
				if (metadata.PrimaryKeys.Contains(columnName))
				{
					continue;
				}
				if (excludedColumns.Contains(columnName))
				{
					continue;
				}

				string parameterName = "@p" + parameterIndex;
				parameterIndex++;
				setClauses.Add(EscapeIdentifier(columnName) + " = " + parameterName);
				object? value;
				row.TryGetValue(columnName, out value);
				sqlParameters.Add(CreateSqlParameter(parameterName, value));
			}

			if (setClauses.Count == 0)
			{
				return new SqlCommand(string.Empty, SqlConnection, transaction);
			}

			List<string> whereClauses = new List<string>();
			for (int i = 0; i < metadata.PrimaryKeyColumns.Count; i++)
			{
				string pkColumnName = metadata.PrimaryKeyColumns[i];
				string parameterName = "@pk" + i;
				whereClauses.Add(EscapeIdentifier(pkColumnName) + " = " + parameterName);
				object? pkValue;
				row.TryGetValue(pkColumnName, out pkValue);
				sqlParameters.Add(CreateSqlParameter(parameterName, pkValue));
			}

			string sql = "UPDATE " + EscapeIdentifier(table.TableName) + " SET " + string.Join(", ", setClauses) + " WHERE " + string.Join(" AND ", whereClauses);
			SqlCommand sqlCommand = new SqlCommand(sql, SqlConnection, transaction);
			for (int i = 0; i < sqlParameters.Count; i++)
			{
				sqlCommand.Parameters.Add(sqlParameters[i]);
			}
			return sqlCommand;
		}

		SqlCommand BuildDeleteCommand(IDataTable table, TableWriteMetadata metadata, IDataRow row, SqlServerSaveChangesOptions options, SqlTransaction? transaction)
		{
			EnsurePrimaryKeys(metadata, table.TableName);

			List<string> whereClauses = new List<string>();
			List<SqlParameter> sqlParameters = new List<SqlParameter>();
			for (int i = 0; i < metadata.PrimaryKeyColumns.Count; i++)
			{
				string pkColumnName = metadata.PrimaryKeyColumns[i];
				string parameterName = "@pk" + i;
				whereClauses.Add(EscapeIdentifier(pkColumnName) + " = " + parameterName);
				object? pkValue;
				row.TryGetValue(pkColumnName, out pkValue);
				sqlParameters.Add(CreateSqlParameter(parameterName, pkValue));
			}

			string sql = "DELETE FROM " + EscapeIdentifier(table.TableName) + " WHERE " + string.Join(" AND ", whereClauses);
			SqlCommand sqlCommand = new SqlCommand(sql, SqlConnection, transaction);
			for (int i = 0; i < sqlParameters.Count; i++)
			{
				sqlCommand.Parameters.Add(sqlParameters[i]);
			}
			return sqlCommand;
		}

		void EnsurePrimaryKeys(TableWriteMetadata metadata, string tableName)
		{
			if (metadata.PrimaryKeyColumns.Count == 0)
			{
				throw new InvalidOperationException("Table '" + tableName + "' has no primary keys in SQL Server.");
			}
		}

		HashSet<string> GetExcludedColumns(string tableName, SqlServerSaveChangesOptions options)
		{
			HashSet<string> excludedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (options.ExcludedColumnsByTable == null)
			{
				return excludedColumns;
			}

			HashSet<string>? configured;
			options.ExcludedColumnsByTable.TryGetValue(tableName, out configured);
			if (configured == null)
			{
				return excludedColumns;
			}

			foreach (string col in configured)
			{
				excludedColumns.Add(col);
			}

			return excludedColumns;
		}

		SqlParameter CreateSqlParameter(string parameterName, object? value)
		{
			SqlParameter sqlParameter = new SqlParameter();
			sqlParameter.ParameterName = parameterName;
			if (value == null)
			{
				sqlParameter.Value = DBNull.Value;
			}
			else
			{
				sqlParameter.Value = value;
			}
			return sqlParameter;
		}

		string EscapeIdentifier(string identifier)
		{
			// Simple SQL Server escaping.
			return "[" + identifier.Replace("]", "]]") + "]";
		}
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
            Dictionary<string, IForeignKeyData> foreignKeysData = new Dictionary<string, IForeignKeyData>();

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
                IForeignKeyData foreignKeyData = new ForeignKeyData();

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
