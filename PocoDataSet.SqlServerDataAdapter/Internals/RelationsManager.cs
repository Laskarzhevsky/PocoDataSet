using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Manages relations
    /// </summary>
    internal static class RelationsManager
    {
        #region Public Methods
        /// <summary>
        /// Loads data table foreign keys
        /// </summary>
        /// <param name="dataTableCreator">Data table creator</param>
        /// <param name="sqlConnection">SQL connection</param>
        public static async Task LoadDataTableForeignKeysAsync(DataTableCreator dataTableCreator, SqlConnection sqlConnection)
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

            using SqlCommand sqlCommand = new SqlCommand(sqlStatement, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@tableName", dataTableCreator.DataTable!.TableName);

            using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
            while (await sqlDataReader.ReadAsync())
            {
                IForeignKeyData foreignKeyData = new ForeignKeyData();

                foreignKeyData.ForeignKeyName = sqlDataReader.GetString(0);
                foreignKeyData.PrincipalTableName = sqlDataReader.GetString(1);
                foreignKeyData.PrincipalColumnName = sqlDataReader.GetString(2);
                foreignKeyData.ReferencedTableName = sqlDataReader.GetString(3);
                foreignKeyData.ReferencedColumnName = sqlDataReader.GetString(4);

                foreignKeysData[foreignKeyData.PrincipalColumnName] = foreignKeyData;
            }

            dataTableCreator.ForeignKeysData = foreignKeysData;
        }

        /// <summary>
        /// Loads data table primary keys
        /// </summary>
        /// <param name="dataTableCreator">Data table creator</param>
        /// <param name="sqlConnection">SQL connection</param>
        public static async Task LoadDataTablePrimaryKeysAsync(DataTableCreator dataTableCreator, SqlConnection sqlConnection)
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

            using SqlCommand sqlCommand = new SqlCommand(sqlStatement, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@tableName", dataTableCreator.DataTable!.TableName);

            using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
            while (await sqlDataReader.ReadAsync())
            {
                var columnName = sqlDataReader.GetString(0);
                primaryKeysData.Add(columnName);
            }

            dataTableCreator.PrimaryKeyData = primaryKeysData;
        }

        /// <summary>
        /// Populates relations in the filled data set by reading database schema (foreign keys).
        /// This supports composite foreign keys and relations that reference alternate (unique) keys.
        /// </summary>
        /// <param name="dataSet">Filled data set</param>
        public static async Task PopulateRelationsFromDatabaseSchemaAsync(IDataSet dataSet, SqlConnection sqlConnection)
        {
            if (dataSet == null)
            {
                return;
            }

            if (sqlConnection.State != System.Data.ConnectionState.Open)
            {
                await sqlConnection.OpenAsync().ConfigureAwait(false);
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

            List<ForeignKeyGroup> foreignKeyGroups = await LoadForeignKeyGroupsAsync(tableNames, sqlConnection).ConfigureAwait(false);
            ApplyForeignKeyGroupsToDataSetRelations(dataSet, foreignKeyGroups);
        }
        #endregion

        #region Private Methods
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

                if (!dataSet.Tables.ContainsKey(foreignKeyGroup.PrincipalTableName))
                {
                    continue;
                }

                if (!dataSet.Tables.ContainsKey(foreignKeyGroup.ReferencedTableName))
                {
                    continue;
                }

                if (foreignKeyGroup.PrincipalColumnNames.Count == 0 || foreignKeyGroup.ReferencedColumnNames.Count == 0)
                {
                    continue;
                }

                if (foreignKeyGroup.PrincipalColumnNames.Count != foreignKeyGroup.ReferencedColumnNames.Count)
                {
                    continue;
                }

                // Ensure all columns exist in both tables
                IDataTable childTable = dataSet.Tables[foreignKeyGroup.PrincipalTableName];
                IDataTable parentTable = dataSet.Tables[foreignKeyGroup.ReferencedTableName];

                bool allChildColumnsExist = true;
                foreach (string childColumnName in foreignKeyGroup.PrincipalColumnNames)
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
                    childTableName: foreignKeyGroup.PrincipalTableName,
                    childColumnNames: foreignKeyGroup.PrincipalColumnNames);
            }
        }

        /// <summary>
        /// Loads foreign key groups for the specified table names.
        /// Parent = dependent table, Referenced = principal table.
        /// </summary>
        /// <param name="tableNames">Table names</param>
        /// <returns>List of foreign key groups</returns>
        static async Task<List<ForeignKeyGroup>> LoadForeignKeyGroupsAsync(HashSet<string> tableNames, SqlConnection sqlConnection)
        {
            List<ForeignKeyGroup> result = new List<ForeignKeyGroup>();

            // Build an IN list with parameters to avoid SQL injection.
            List<string> parameters = new List<string>();
            using SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = sqlConnection;

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
                    group.PrincipalTableName = parentTable;
                    group.ReferencedTableName = referencedTable;
                    groups[key] = group;
                }

                group.PrincipalColumnNames.Add(parentColumn);
                group.ReferencedColumnNames.Add(referencedColumn);
            }

            foreach (ForeignKeyGroup group in groups.Values)
            {
                result.Add(group);
            }

            return result;
        }

        #endregion
    }
}
