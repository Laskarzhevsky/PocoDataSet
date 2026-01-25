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
        #region Methods

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

            bool hasAnyRows = false;
            foreach (KeyValuePair<string, IDataTable> kv in changeset.Tables)
            {
                IDataTable table = kv.Value;
                if (table != null && table.Rows != null && table.Rows.Count > 0)
                {
                    hasAnyRows = true;
                    break;
                }
            }

            if (!hasAnyRows)
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

			int affectedRows = 0;

			SqlConnection = new SqlConnection(ConnectionString);
			await SqlConnection.OpenAsync().ConfigureAwait(false);

			SqlTransaction transaction = SqlConnection.BeginTransaction();
			try
			{
				Dictionary<string, TableWriteMetadata> metadataCache = new Dictionary<string, TableWriteMetadata>(StringComparer.OrdinalIgnoreCase);

				List<IDataTable> tablesWithChanges = await BuildOrderedTablesWithChangesByForeignKeysAsync(changeset, transaction).ConfigureAwait(false);

				for (int t = 0; t < tablesWithChanges.Count; t++)
				{
					IDataTable table = tablesWithChanges[t];
					ValidateTableForSave(table);

					// Load and validate SQL Server schema once per table
					TableWriteMetadata metadata = await GetOrLoadTableWriteMetadataAsync(metadataCache, table.TableName, transaction).ConfigureAwait(false);
					ValidateTableExistsInSqlServer(metadata, table.TableName);
				}

				affectedRows += await ApplyDeletesAsync(tablesWithChanges, metadataCache, transaction).ConfigureAwait(false);
				affectedRows += await ApplyInsertsAsync(tablesWithChanges, metadataCache, transaction).ConfigureAwait(false);
				affectedRows += await ApplyUpdatesAsync(tablesWithChanges, metadataCache, transaction).ConfigureAwait(false);
				transaction.Commit();
			}
			catch
			{
				try
				{
					transaction.Rollback();
				}
				catch
				{
					// ignore rollback exceptions
				}
				throw;
			}
			finally
			{
				await transaction.DisposeAsync().ConfigureAwait(false);
				await DisposeAsync().ConfigureAwait(false);
			}

			return affectedRows;
		}

		/// <summary>
		/// Builds ordered list of tables that contain changes (Added/Modified/Deleted),
		/// ordered automatically by SQL Server foreign key relationships.
		/// </summary>
		async Task<List<IDataTable>> BuildOrderedTablesWithChangesByForeignKeysAsync(IDataSet changeset, SqlTransaction transaction)
		{
			List<IDataTable> tablesWithChanges = new List<IDataTable>();
			Dictionary<string, IDataTable> tableByName = new Dictionary<string, IDataTable>(StringComparer.OrdinalIgnoreCase);

			foreach (KeyValuePair<string, IDataTable> kvp in changeset.Tables)
			{
				IDataTable table = kvp.Value;
				if (TableHasChanges(table))
				{
					tablesWithChanges.Add(table);
					if (!tableByName.ContainsKey(table.TableName))
					{
						tableByName.Add(table.TableName, table);
					}
				}
			}

			if (tablesWithChanges.Count == 0)
			{
				return tablesWithChanges;
			}

			HashSet<string> tableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < tablesWithChanges.Count; i++)
			{
				tableNames.Add(tablesWithChanges[i].TableName);
			}

			List<ForeignKeyEdge> edges = await LoadForeignKeyEdgesAsync(tableNames, transaction).ConfigureAwait(false);

			// Build a stable order map based on incoming list order (used as tie-breaker).
			Dictionary<string, int> orderIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < tablesWithChanges.Count; i++)
			{
				string name = tablesWithChanges[i].TableName;
				if (!orderIndex.ContainsKey(name))
				{
					orderIndex.Add(name, i);
				}
			}

			List<string> orderedNames = OrderTableNamesByForeignKeys(tableNames, edges, orderIndex);

			List<IDataTable> orderedTables = new List<IDataTable>();
			for (int i = 0; i < orderedNames.Count; i++)
			{
				IDataTable? table;
				tableByName.TryGetValue(orderedNames[i], out table);
				if (table != null)
				{
					orderedTables.Add(table);
				}
			}

			return orderedTables;
		}

		bool TableHasChanges(IDataTable table)
		{
			for (int i = 0; i < table.Rows.Count; i++)
			{
				IDataRow row = table.Rows[i];
				if (row.DataRowState == PocoDataSet.IData.DataRowState.Added ||
					row.DataRowState == PocoDataSet.IData.DataRowState.Modified ||
					row.DataRowState == PocoDataSet.IData.DataRowState.Deleted)
				{
					return true;
				}
			}
			return false;
		}

		class ForeignKeyEdge
		{
			public string ParentTable
			{
				get; set;
			} = string.Empty;

			public string ChildTable
			{
				get; set;
			} = string.Empty;

			public string ForeignKeyName
			{
				get; set;
			} = string.Empty;
		}

		async Task<List<ForeignKeyEdge>> LoadForeignKeyEdgesAsync(HashSet<string> tableNames, SqlTransaction transaction)
		{
			List<ForeignKeyEdge> edges = new List<ForeignKeyEdge>();

			// Load all FK relations between user tables; filter in-memory to our table set.
			string sql = @"
SELECT
	fk.name AS ForeignKeyName,
	parent.name AS ChildTable,
	referenced.name AS ParentTable
FROM sys.foreign_keys fk
JOIN sys.tables parent ON parent.object_id = fk.parent_object_id
JOIN sys.tables referenced ON referenced.object_id = fk.referenced_object_id
WHERE parent.is_ms_shipped = 0
  AND referenced.is_ms_shipped = 0;";

			using (SqlCommand cmd = new SqlCommand(sql, SqlConnection, transaction))
			{
				using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
				{
					while (await reader.ReadAsync().ConfigureAwait(false))
					{
						string fkName = reader.GetString(0);
						string child = reader.GetString(1);
						string parent = reader.GetString(2);

						if (!tableNames.Contains(child))
						{
							continue;
						}
						if (!tableNames.Contains(parent))
						{
							continue;
						}

						ForeignKeyEdge edge = new ForeignKeyEdge();
						edge.ForeignKeyName = fkName;
						edge.ChildTable = child;
						edge.ParentTable = parent;
						edges.Add(edge);
					}
				}
			}

			return edges;
		}

		List<string> OrderTableNamesByForeignKeys(HashSet<string> tableNames, List<ForeignKeyEdge> edges, Dictionary<string, int> orderIndex)
		{
			// parent -> children adjacency
			Dictionary<string, List<string>> adjacency = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, int> indegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			foreach (string name in tableNames)
			{
				adjacency[name] = new List<string>();
				indegree[name] = 0;
			}

			for (int i = 0; i < edges.Count; i++)
			{
				ForeignKeyEdge edge = edges[i];

				// Parent -> Child
				adjacency[edge.ParentTable].Add(edge.ChildTable);
				indegree[edge.ChildTable] = indegree[edge.ChildTable] + 1;
			}

			// Start nodes (indegree 0) - maintain stable order using orderIndex
			List<string> ready = new List<string>();
			foreach (KeyValuePair<string, int> kvp in indegree)
			{
				if (kvp.Value == 0)
				{
					ready.Add(kvp.Key);
				}
			}

			SortByIncomingOrder(ready, orderIndex);

			List<string> result = new List<string>();

			while (ready.Count > 0)
			{
				string current = ready[0];
				ready.RemoveAt(0);
				result.Add(current);

				List<string> children = adjacency[current];
				for (int i = 0; i < children.Count; i++)
				{
					string child = children[i];
					indegree[child] = indegree[child] - 1;
					if (indegree[child] == 0)
					{
						ready.Add(child);
					}
				}

				SortByIncomingOrder(ready, orderIndex);
			}

			if (result.Count != tableNames.Count)
			{
				// cycle detected
				List<string> remaining = new List<string>();
				foreach (KeyValuePair<string, int> kvp in indegree)
				{
					if (kvp.Value > 0)
					{
						remaining.Add(kvp.Key);
					}
				}
				SortByIncomingOrder(remaining, orderIndex);

				throw new InvalidOperationException(
					"Cannot automatically order tables by foreign keys due to a cycle. Tables in cycle: " + string.Join(", ", remaining));
			}

			return result;
		}

		void SortByIncomingOrder(List<string> list, Dictionary<string, int> orderIndex)
		{
			// Simple stable insertion sort by orderIndex
			for (int i = 1; i < list.Count; i++)
			{
				string key = list[i];
				int keyOrder = GetOrderIndex(orderIndex, key);

				int j = i - 1;
				while (j >= 0 && GetOrderIndex(orderIndex, list[j]) > keyOrder)
				{
					list[j + 1] = list[j];
					j--;
				}

				list[j + 1] = key;
			}
		}

		int GetOrderIndex(Dictionary<string, int> orderIndex, string tableName)
		{
			int idx;
			if (orderIndex.TryGetValue(tableName, out idx))
			{
				return idx;
			}
			return int.MaxValue;
		}


		void ValidateTableExistsInSqlServer(TableWriteMetadata metadata, string tableName)
		{
			if (metadata.ColumnNames.Count == 0)
			{
				throw new InvalidOperationException("Table '" + tableName + "' does not exist in SQL Server or is not accessible.");
			}
		}



		class TableWriteMetadata
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

		HashSet<string> BuildExcludedColumns(IDataTable table, TableWriteMetadata metadata)
		{
            HashSet<string> excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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



		List<string> BuildInsertOutputColumns(TableWriteMetadata metadata)
		{
			List<string> outputColumns = new List<string>();

			List<string> identityColumns = new List<string>(metadata.IdentityColumns);
			identityColumns.Sort(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < identityColumns.Count; i++)
			{
				outputColumns.Add(identityColumns[i]);
			}

			List<string> rvColumns = new List<string>(metadata.RowVersionColumns);
			rvColumns.Sort(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < rvColumns.Count; i++)
			{
				outputColumns.Add(rvColumns[i]);
			}

			return outputColumns;
		}

		List<string> BuildUpdateOutputColumns(TableWriteMetadata metadata)
		{
			List<string> outputColumns = new List<string>();

			List<string> rvColumns = new List<string>(metadata.RowVersionColumns);
			rvColumns.Sort(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < rvColumns.Count; i++)
			{
				outputColumns.Add(rvColumns[i]);
			}

			return outputColumns;
		}

		void ApplyOutputValuesToRow(IDataRow row, List<string> outputColumns, SqlDataReader reader)
		{
			for (int i = 0; i < outputColumns.Count; i++)
			{
				object value = reader.GetValue(i);
				if (value == DBNull.Value)
				{
					row[outputColumns[i]] = null;
				}
				else
				{
					row[outputColumns[i]] = value;
				}
			}
		}

		string BuildPrimaryKeyText(TableWriteMetadata metadata, IDataRow row)
		{
			List<string> parts = new List<string>();

			for (int i = 0; i < metadata.PrimaryKeyColumns.Count; i++)
			{
				string pk = metadata.PrimaryKeyColumns[i];
				object? value;
				if (!row.TryGetValue(pk, out value))
				{
					row.TryGetOriginalValue(pk, out value);
				}

				string text;
				if (value == null)
				{
					text = pk + "=null";
				}
				else
				{
					text = pk + "=" + value.ToString();
				}

				parts.Add(text);
			}

			return string.Join(", ", parts);
		}

		void AddRowVersionConcurrencyIfPossible(TableWriteMetadata metadata, IDataRow row, List<string> whereClauses, List<SqlParameter> sqlParameters)
		{
			if (metadata.RowVersionColumns.Count != 1)
			{
				return;
			}

			string rvColumn = null!;
			foreach (string col in metadata.RowVersionColumns)
			{
				rvColumn = col;
				break;
			}

			object? rvValue = null;
			bool hasRv = false;

			// Prefer the OriginalValues snapshot when available (classic full changeset scenario)
			if (row.HasOriginalValues && row.OriginalValues != null)
			{
				hasRv = row.OriginalValues.TryGetValue(rvColumn, out rvValue);
			}

			// Delta / floating rows may not have OriginalValues; rowversion should be provided directly.
			if (!hasRv)
			{
				if (row.TryGetValue(rvColumn, out rvValue))
				{
					hasRv = true;
				}
				else if (row.TryGetOriginalValue(rvColumn, out rvValue))
				{
					hasRv = true;
				}
			}

			if (!hasRv)
			{
				throw new InvalidOperationException("Optimistic concurrency requested but rowversion value is missing for column '" + rvColumn + "'.");
			}

			string parameterName = "@oc_rv";
			whereClauses.Add(EscapeIdentifier(rvColumn) + " = " + parameterName);
			sqlParameters.Add(CreateSqlParameter(parameterName, rvValue));
		}

		void AddOriginalValuesConcurrencyClauses(IDataTable table, TableWriteMetadata metadata, IDataRow row, List<string> whereClauses, List<SqlParameter> sqlParameters)
		{
			if (!row.HasOriginalValues)
			{
				throw new InvalidOperationException("Optimistic concurrency requested but OriginalValues snapshot is missing.");
			}

			HashSet<string> excludedColumns = BuildExcludedColumns(table, metadata);

			List<string> autoColumns = new List<string>();
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
				autoColumns.Add(columnName);
			}
			autoColumns.Sort(StringComparer.OrdinalIgnoreCase);

			for (int i = 0; i < autoColumns.Count; i++)
			{
				AddSingleOriginalValueConcurrencyClause(autoColumns[i], row, whereClauses, sqlParameters);
			}
		}

		void AddSingleOriginalValueConcurrencyClause(string columnName, IDataRow row, List<string> whereClauses, List<SqlParameter> sqlParameters)
		{
			object? origValue;
			if (!row.OriginalValues.TryGetValue(columnName, out origValue))
			{
				throw new InvalidOperationException("Optimistic concurrency requested but OriginalValues does not contain column '" + columnName + "'.");
			}

			string parameterName = "@oc" + sqlParameters.Count;
			string escaped = EscapeIdentifier(columnName);
			whereClauses.Add("(" + escaped + " = " + parameterName + " OR (" + escaped + " IS NULL AND " + parameterName + " IS NULL))");
			sqlParameters.Add(CreateSqlParameter(parameterName, origValue));
		}


		async Task<int> ApplyDeletesAsync(List<IDataTable> orderedTables, Dictionary<string, TableWriteMetadata> metadataCache, SqlTransaction? transaction)
		{
			int affected = 0;

			for (int t = orderedTables.Count - 1; t >= 0; t--)
			{
				IDataTable table = orderedTables[t];
				TableWriteMetadata metadata = metadataCache[table.TableName];

				for (int i = 0; i < table.Rows.Count; i++)
				{
					IDataRow row = table.Rows[i];
					if (row.DataRowState == PocoDataRowState.Detached)
					{
						continue;
					}
					if (row.DataRowState != PocoDataRowState.Deleted)
					{
						continue;
					}

					using SqlCommand sqlCommand = BuildDeleteCommand(table, metadata, row, transaction);
					int localAffected = await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
					if (localAffected == 0)
					{
						throw new PocoConcurrencyException(table.TableName, "DELETE", BuildPrimaryKeyText(metadata, row));
					}

					affected += localAffected;
				}
			}

			return affected;
		}

		async Task<int> ApplyInsertsAsync(List<IDataTable> orderedTables, Dictionary<string, TableWriteMetadata> metadataCache, SqlTransaction? transaction)
		{
			int affected = 0;

			for (int t = 0; t < orderedTables.Count; t++)
			{
				IDataTable table = orderedTables[t];
				TableWriteMetadata metadata = metadataCache[table.TableName];

				List<string> outputColumnsForTable = BuildInsertOutputColumns(metadata);

				for (int i = 0; i < table.Rows.Count; i++)
				{
					IDataRow row = table.Rows[i];
					if (row.DataRowState == PocoDataRowState.Detached)
					{
						continue;
					}
					if (row.DataRowState != PocoDataRowState.Added)
					{
						continue;
					}

					using SqlCommand sqlCommand = BuildInsertCommand(table, metadata, row, transaction, outputColumnsForTable);

					if (outputColumnsForTable.Count == 0)
					{
						affected += await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
						continue;
					}

					using (SqlDataReader reader = await sqlCommand.ExecuteReaderAsync().ConfigureAwait(false))
					{
						if (await reader.ReadAsync().ConfigureAwait(false))
						{
							ApplyOutputValuesToRow(row, outputColumnsForTable, reader);
							affected += 1;
						}
						else
						{
							throw new InvalidOperationException("INSERT did not return output values for table '" + table.TableName + "'.");
						}
					}
				}
			}

			return affected;
		}

		async Task<int> ApplyUpdatesAsync(List<IDataTable> orderedTables, Dictionary<string, TableWriteMetadata> metadataCache, SqlTransaction? transaction)
		{
			int affected = 0;

			for (int t = 0; t < orderedTables.Count; t++)
			{
				IDataTable table = orderedTables[t];
				TableWriteMetadata metadata = metadataCache[table.TableName];

				List<string> outputColumnsForTable = BuildUpdateOutputColumns(metadata);

				for (int i = 0; i < table.Rows.Count; i++)
				{
					IDataRow row = table.Rows[i];
					if (row.DataRowState == PocoDataRowState.Detached)
					{
						continue;
					}
					if (row.DataRowState != PocoDataRowState.Modified)
					{
						continue;
					}

					using SqlCommand sqlCommand = BuildUpdateCommand(table, metadata, row, transaction, outputColumnsForTable);
					if (sqlCommand.CommandText.Length == 0)
					{
						continue;
					}

					if (outputColumnsForTable.Count == 0)
					{
						int localAffected = await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
						if (localAffected == 0)
						{
							throw new PocoConcurrencyException(table.TableName, "UPDATE", BuildPrimaryKeyText(metadata, row));
						}

						affected += localAffected;
						continue;
					}

					using (SqlDataReader reader = await sqlCommand.ExecuteReaderAsync().ConfigureAwait(false))
					{
						if (await reader.ReadAsync().ConfigureAwait(false))
						{
							ApplyOutputValuesToRow(row, outputColumnsForTable, reader);
							affected += 1;
						}
						else
						{
							throw new PocoConcurrencyException(table.TableName, "UPDATE", BuildPrimaryKeyText(metadata, row));
						}
					}
				}
			}

			return affected;
		}

		void ValidateTableForSave(IDataTable table)
		{
			if (table == null)
			{
				throw new ArgumentNullException(nameof(table));
			}

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


		SqlCommand BuildInsertCommand(IDataTable table, TableWriteMetadata metadata, IDataRow row, SqlTransaction? transaction, List<string> outputColumns)
		{
			List<string> columnNames = new List<string>();
			List<string> parameterNames = new List<string>();
			List<SqlParameter> sqlParameters = new List<SqlParameter>();

			HashSet<string> excludedColumns = BuildExcludedColumns(table, metadata);
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

				object? value;
				if (!row.TryGetValue(columnName, out value))
				{
					// Floating/delta rows: missing field means "not provided" -> omit it from INSERT.
					continue;
				}

				string parameterName = "@p" + parameterIndex;
				parameterIndex++;

				columnNames.Add(EscapeIdentifier(columnName));
				parameterNames.Add(parameterName);
				sqlParameters.Add(CreateSqlParameter(parameterName, value));
			}

			if (columnNames.Count == 0)
			{
				throw new InvalidOperationException("Cannot INSERT into table '" + table.TableName + "': no writable columns found.");
			}

			string outputClause = string.Empty;
			if (outputColumns != null && outputColumns.Count > 0)
			{
				List<string> outputParts = new List<string>();
				for (int i = 0; i < outputColumns.Count; i++)
				{
					outputParts.Add("INSERTED." + EscapeIdentifier(outputColumns[i]));
				}
				outputClause = " OUTPUT " + string.Join(", ", outputParts) + " ";
			}

			string sql = "INSERT INTO " + EscapeIdentifier(table.TableName) + " (" + string.Join(",", columnNames) + ")" + outputClause + "VALUES (" + string.Join(",", parameterNames) + ")";
			SqlCommand sqlCommand = new SqlCommand(sql, SqlConnection, transaction);

			for (int i = 0; i < sqlParameters.Count; i++)
			{
				sqlCommand.Parameters.Add(sqlParameters[i]);
			}

			return sqlCommand;
		}



		SqlCommand BuildUpdateCommand(IDataTable table, TableWriteMetadata metadata, IDataRow row, SqlTransaction? transaction, List<string> outputColumns)
		{
			EnsurePrimaryKeys(metadata, table.TableName);

			HashSet<string> excludedColumns = BuildExcludedColumns(table, metadata);

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

				object? value;
				if (!row.TryGetValue(columnName, out value))
				{
					// Floating/delta rows: missing field means "not provided" -> do not update it.
					continue;
				}

				string parameterName = "@p" + parameterIndex;
				parameterIndex++;

				setClauses.Add(EscapeIdentifier(columnName) + " = " + parameterName);
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
				if (!row.TryGetValue(pkColumnName, out pkValue))
				{
					row.TryGetOriginalValue(pkColumnName, out pkValue);
				}
				sqlParameters.Add(CreateSqlParameter(parameterName, pkValue));
			}

			if (metadata.RowVersionColumns.Count == 1)
			{
				AddRowVersionConcurrencyIfPossible(metadata, row, whereClauses, sqlParameters);
			}
			else
			{
				AddOriginalValuesConcurrencyClauses(table, metadata, row, whereClauses, sqlParameters);
			}

			string outputClause = string.Empty;
			if (outputColumns != null && outputColumns.Count > 0)
			{
				List<string> outputParts = new List<string>();
				for (int i = 0; i < outputColumns.Count; i++)
				{
					outputParts.Add("INSERTED." + EscapeIdentifier(outputColumns[i]));
				}
				outputClause = " OUTPUT " + string.Join(", ", outputParts) + " ";
			}

			string sql = "UPDATE " + EscapeIdentifier(table.TableName) + " SET " + string.Join(", ", setClauses) + outputClause + "WHERE " + string.Join(" AND ", whereClauses);
			SqlCommand sqlCommand = new SqlCommand(sql, SqlConnection, transaction);

			for (int i = 0; i < sqlParameters.Count; i++)
			{
				sqlCommand.Parameters.Add(sqlParameters[i]);
			}

			return sqlCommand;
		}



		SqlCommand BuildDeleteCommand(IDataTable table, TableWriteMetadata metadata, IDataRow row, SqlTransaction? transaction)
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
				if (!row.TryGetValue(pkColumnName, out pkValue))
				{
					row.TryGetOriginalValue(pkColumnName, out pkValue);
				}
				sqlParameters.Add(CreateSqlParameter(parameterName, pkValue));
			}

			HashSet<string> excludedColumns = BuildExcludedColumns(table, metadata);

			if (metadata.RowVersionColumns.Count == 1)
			{
				AddRowVersionConcurrencyIfPossible(metadata, row, whereClauses, sqlParameters);
			}
			else
			{
				AddOriginalValuesConcurrencyClauses(table, metadata, row, whereClauses, sqlParameters);
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
        /// Populates relations in the filled data set by reading database schema (foreign keys).
        /// This supports composite foreign keys and relations that reference alternate (unique) keys.
        /// </summary>
        /// <param name="dataSet">Filled data set</param>
        async Task PopulateRelationsFromDatabaseSchemaAsync(IDataSet dataSet)
        {
            if (dataSet == null)
            {
                return;
            }

            if (SqlConnection == null)
            {
                return;
            }

            if (SqlConnection.State != System.Data.ConnectionState.Open)
            {
                await SqlConnection.OpenAsync().ConfigureAwait(false);
            }

            HashSet<string> tableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string tableName in dataSet.Tables.Keys)
            {
                tableNames.Add(tableName);
            }

            if (tableNames.Count == 0)
            {
                return;
            }

            List<ForeignKeyGroup> foreignKeyGroups = await LoadForeignKeyGroupsAsync(tableNames).ConfigureAwait(false);
            ApplyForeignKeyGroupsToDataSetRelations(dataSet, foreignKeyGroups);
        }

        /// <summary>
        /// Applies foreign key groups to the data set relations.
        /// Parent = dependent table, Referenced = principal table.
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="foreignKeyGroups">Foreign key groups</param>
        internal static void ApplyForeignKeyGroupsToDataSetRelations(IDataSet dataSet, IList<ForeignKeyGroup> foreignKeyGroups)
        {
            if (dataSet == null)
            {
                return;
            }

            if (foreignKeyGroups == null || foreignKeyGroups.Count == 0)
            {
                return;
            }

            foreach (ForeignKeyGroup foreignKeyGroup in foreignKeyGroups)
            {
                if (string.IsNullOrWhiteSpace(foreignKeyGroup.ForeignKeyName))
                {
                    continue;
                }

                if (!dataSet.Tables.ContainsKey(foreignKeyGroup.ParentTableName))
                {
                    continue;
                }

                if (!dataSet.Tables.ContainsKey(foreignKeyGroup.ReferencedTableName))
                {
                    continue;
                }

                if (foreignKeyGroup.ParentColumnNames.Count == 0 || foreignKeyGroup.ReferencedColumnNames.Count == 0)
                {
                    continue;
                }

                if (foreignKeyGroup.ParentColumnNames.Count != foreignKeyGroup.ReferencedColumnNames.Count)
                {
                    continue;
                }

                // Ensure all columns exist in both tables
                IDataTable childTable = dataSet.Tables[foreignKeyGroup.ParentTableName];
                IDataTable parentTable = dataSet.Tables[foreignKeyGroup.ReferencedTableName];

                bool allChildColumnsExist = true;
                foreach (string childColumnName in foreignKeyGroup.ParentColumnNames)
                {
                    if (!childTable.ContainsColumn(childColumnName))
                    {
                        allChildColumnsExist = false;
                        break;
                    }
                }

                if (!allChildColumnsExist)
                {
                    continue;
                }

                bool allParentColumnsExist = true;
                foreach (string parentColumnName in foreignKeyGroup.ReferencedColumnNames)
                {
                    if (!parentTable.ContainsColumn(parentColumnName))
                    {
                        allParentColumnsExist = false;
                        break;
                    }
                }

                if (!allParentColumnsExist)
                {
                    continue;
                }

                // Relation name must be unique within the dataset.
                string relationName = foreignKeyGroup.ForeignKeyName;
                if (dataSet.Relations != null && dataSet.ContainsRelation(relationName))
                {
                    continue;
                }

                // Parent = referenced table, Child = dependent table
                dataSet.AddRelation(
                    relationName: relationName,
                    parentTableName: foreignKeyGroup.ReferencedTableName,
                    parentColumnNames: foreignKeyGroup.ReferencedColumnNames,
                    childTableName: foreignKeyGroup.ParentTableName,
                    childColumnNames: foreignKeyGroup.ParentColumnNames);
            }
        }

        /// <summary>
        /// Loads foreign key groups for the specified table names.
        /// Parent = dependent table, Referenced = principal table.
        /// </summary>
        /// <param name="tableNames">Table names</param>
        /// <returns>List of foreign key groups</returns>
        async Task<List<ForeignKeyGroup>> LoadForeignKeyGroupsAsync(HashSet<string> tableNames)
        {
            List<ForeignKeyGroup> result = new List<ForeignKeyGroup>();

            // Build an IN list with parameters to avoid SQL injection.
            List<string> parameters = new List<string>();
            using SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = SqlConnection;

            int index = 0;
            foreach (string tableName in tableNames)
            {
                string paramName = "@t" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
                parameters.Add(paramName);
                sqlCommand.Parameters.AddWithValue(paramName, tableName);
                index++;
            }

            string inList = string.Join(", ", parameters);

            sqlCommand.CommandText = @"
                SELECT
                    fk.name AS ForeignKeyName,
                    parent.name AS ParentTable,
                    referenced.name AS ReferencedTable,
                    fkc.constraint_column_id AS ColumnOrder,
                    pc.name AS ParentColumn,
                    rc.name AS ReferencedColumn
                FROM sys.foreign_keys fk
                JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                JOIN sys.tables parent ON parent.object_id = fk.parent_object_id
                JOIN sys.columns pc ON pc.object_id = parent.object_id AND pc.column_id = fkc.parent_column_id
                JOIN sys.tables referenced ON referenced.object_id = fk.referenced_object_id
                JOIN sys.columns rc ON rc.object_id = referenced.object_id AND rc.column_id = fkc.referenced_column_id
                WHERE parent.name IN (" + inList + @") AND referenced.name IN (" + inList + @")
                ORDER BY fk.name, parent.name, referenced.name, fkc.constraint_column_id;";

            Dictionary<string, ForeignKeyGroup> groups = new Dictionary<string, ForeignKeyGroup>(StringComparer.OrdinalIgnoreCase);

            using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync().ConfigureAwait(false);
            while (await sqlDataReader.ReadAsync().ConfigureAwait(false))
            {
                string foreignKeyName = sqlDataReader.GetString(0);
                string parentTable = sqlDataReader.GetString(1);
                string referencedTable = sqlDataReader.GetString(2);
                string parentColumn = sqlDataReader.GetString(4);
                string referencedColumn = sqlDataReader.GetString(5);

                string key = foreignKeyName + "|" + parentTable + "|" + referencedTable;
                if (!groups.TryGetValue(key, out ForeignKeyGroup? group))
                {
                    group = new ForeignKeyGroup();
                    group.ForeignKeyName = foreignKeyName;
                    group.ParentTableName = parentTable;
                    group.ReferencedTableName = referencedTable;
                    groups[key] = group;
                }

                group.ParentColumnNames.Add(parentColumn);
                group.ReferencedColumnNames.Add(referencedColumn);
            }

            foreach (ForeignKeyGroup group in groups.Values)
            {
                result.Add(group);
            }

            return result;
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
