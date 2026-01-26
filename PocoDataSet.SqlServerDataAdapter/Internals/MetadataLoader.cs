using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

namespace PocoDataSet.SqlServerDataAdapter
{
    internal static class MetadataLoader
    {
        #region Public Methods
        /// <summary>
        /// Gets or loads table write metadata async
        /// </summary>
        /// <param name="tableWriteMetadataCache">Table write metadata cahce</param>
        /// <param name="tableNames">Table names</param>
        /// <param name="sqlConnection">SQL connection</param>
        /// <param name="sqlTransaction">SQL transaction</param>
        /// <returns>Table write metadata</returns>
		public static async Task<TableWriteMetadata> GetOrLoadTableWriteMetadataAsync(Dictionary<string, TableWriteMetadata> tableWriteMetadataCache, string tableName, SqlConnection sqlConnection, SqlTransaction? sqlTransaction)
        {
            TableWriteMetadata? metadata;
            tableWriteMetadataCache.TryGetValue(tableName, out metadata);
            if (metadata != null)
            {
                return metadata;
            }

            metadata = await MetadataLoader.LoadTableWriteMetadataAsync(tableName, sqlConnection, sqlTransaction).ConfigureAwait(false);
            tableWriteMetadataCache[tableName] = metadata;
            return metadata;
        }

        /// <summary>
        /// Loads foreign key edges async
        /// </summary>
        /// <param name="tableNames">Table names</param>
        /// <param name="sqlConnection">SQL connection</param>
        /// <param name="sqlTransaction">SQL transaction</param>
        /// <returns>Loaded foreign key edges</returns>
        public static async Task<List<ForeignKeyEdge>> LoadForeignKeyEdgesAsync(HashSet<string> tableNames, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
        {
            List<ForeignKeyEdge> edges = new List<ForeignKeyEdge>();

            // Load all FK relations between user tables; filter in-memory to our table set.
            string sql = @"
SELECT
	fk.name AS ForeignKeyName,
	parent.name AS DependentTable,
	referenced.name AS PrincipalTable
FROM sys.foreign_keys fk
JOIN sys.tables parent ON parent.object_id = fk.parent_object_id
JOIN sys.tables referenced ON referenced.object_id = fk.referenced_object_id
WHERE parent.is_ms_shipped = 0
  AND referenced.is_ms_shipped = 0;";

            using (SqlCommand cmd = new SqlCommand(sql, sqlConnection, sqlTransaction))
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
                        edge.ReferencedTableName = child;
                        edge.PrincipalTableName = parent;
                        edges.Add(edge);
                    }
                }
            }

            return edges;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Loads table write metadata async
        /// </summary>
        /// <param name="tableNames">Table names</param>
        /// <param name="sqlConnection">SQL connection</param>
        /// <param name="sqlTransaction">SQL transaction</param>
        /// <returns>Loaded table write metadata</returns>
        static async Task<TableWriteMetadata> LoadTableWriteMetadataAsync(string tableName, SqlConnection sqlConnection, SqlTransaction? transaction)
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

            using (SqlCommand columnsCommand = new SqlCommand(columnsSql, sqlConnection, transaction))
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

            using (SqlCommand pkCommand = new SqlCommand(pkSql, sqlConnection, transaction))
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
        #endregion
    }
}
