using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// EF Core model helpers for aligning PocoDataSet schema (primary keys and relations) with the DbContext model.
    /// </summary>
    public static partial class EfCoreBridgeExtensions
    {
        #region Public Methods
        /// <summary>
        /// Applies primary keys and relations to the target data set based on the EF Core model.
        /// This method is intended to be called after you materialize multiple tables into the same data set
        /// (e.g., via ToDataTable / ToDataTableAsync), so relations between the loaded tables can be created automatically.
        /// </summary>
        /// <param name="dataSet">Target data set.</param>
        /// <param name="dbContext">DbContext to read EF model from.</param>
        public static void ApplyEfModelKeysAndRelations(this IDataSet dataSet, DbContext dbContext)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException(nameof(dataSet));
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            // Build map from table name -> EF entity type. Prefer relational table names when available.
            Dictionary<string, IEntityType> entityTypeByTableName = BuildEntityTypeMap(dbContext);

            // Apply PKs for every present table.
            foreach (IDataTable table in dataSet.Tables.Values)
            {
                IEntityType? entityType = TryResolveEntityTypeForTable(entityTypeByTableName, table.TableName);
                if (entityType == null)
                {
                    continue;
                }

                ApplyPrimaryKeys(table, entityType);
            }

            // Apply relations (FKs) between tables that are present in the data set.
            ApplyRelations(dataSet, dbContext, entityTypeByTableName);
        }
        #endregion

        #region Private Methods
        private static Dictionary<string, IEntityType> BuildEntityTypeMap(DbContext dbContext)
        {
            Dictionary<string, IEntityType> map = new Dictionary<string, IEntityType>(StringComparer.OrdinalIgnoreCase);

            foreach (IEntityType entityType in dbContext.Model.GetEntityTypes())
            {
                // Skip types without CLR type (rare, but safer).
                if (entityType.ClrType == null)
                {
                    continue;
                }

                // Prefer relational table name when provider supports it.
                string? tableName = TryGetRelationalTableName(entityType);
                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    if (!map.ContainsKey(tableName))
                    {
                        map.Add(tableName, entityType);
                    }
                }

                // Also allow resolving by CLR type name as a fallback.
                string clrName = entityType.ClrType.Name;
                if (!map.ContainsKey(clrName))
                {
                    map.Add(clrName, entityType);
                }
            }

            return map;
        }

        private static IEntityType? TryResolveEntityTypeForTable(Dictionary<string, IEntityType> entityTypeByTableName, string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return null;
            }

            if (entityTypeByTableName.TryGetValue(tableName, out IEntityType? entityType))
            {
                return entityType;
            }

            return null;
        }

        private static void ApplyPrimaryKeys(IDataTable table, IEntityType entityType)
        {
            IKey? pk = entityType.FindPrimaryKey();
            if (pk == null)
            {
                return;
            }

            List<string> pkColumns = new List<string>();

            StoreObjectIdentifier? storeObject = TryGetStoreObjectIdentifier(entityType);
            foreach (IProperty property in pk.Properties)
            {
                string columnName = GetRelationalColumnNameOrFallback(property, storeObject);
                if (!string.IsNullOrWhiteSpace(columnName))
                {
                    pkColumns.Add(columnName);
                }
            }

            if (pkColumns.Count == 0)
            {
                return;
            }

            // Only set if different to avoid surprising callers that explicitly set a custom PK strategy.
            if (table.PrimaryKeys == null || table.PrimaryKeys.Count == 0)
            {
                table.PrimaryKeys = pkColumns;
                return;
            }

            if (!AreSameColumns(table.PrimaryKeys, pkColumns))
            {
                table.PrimaryKeys = pkColumns;
            }
        }

        private static void ApplyRelations(IDataSet dataSet, DbContext dbContext, Dictionary<string, IEntityType> entityTypeByTableName)
        {
            HashSet<string> presentTables = new HashSet<string>(dataSet.Tables.Keys, StringComparer.OrdinalIgnoreCase);

            foreach (IEntityType dependentEntityType in dbContext.Model.GetEntityTypes())
            {
                string? dependentTableName = ResolveTableName(dependentEntityType);
                if (string.IsNullOrWhiteSpace(dependentTableName) || !presentTables.Contains(dependentTableName))
                {
                    continue;
                }

                foreach (IForeignKey fk in dependentEntityType.GetForeignKeys())
                {
                    string? principalTableName = ResolveTableName(fk.PrincipalEntityType);
                    if (string.IsNullOrWhiteSpace(principalTableName) || !presentTables.Contains(principalTableName))
                    {
                        continue;
                    }

                    List<string> childColumns = ResolveColumnNames(fk.DeclaringEntityType, dependentTableName, fk.Properties);
                    List<string> parentColumns = ResolveColumnNames(fk.PrincipalEntityType, principalTableName, fk.PrincipalKey.Properties);

                    if (childColumns.Count == 0 || parentColumns.Count == 0)
                    {
                        continue;
                    }

                    // Sanity: composite FK lists must match.
                    if (childColumns.Count != parentColumns.Count)
                    {
                        continue;
                    }

                    // Ensure columns exist in the Poco tables (best effort; if schema isn't present yet, skip).
                    if (!DoAllColumnsExist(dataSet.Tables[dependentTableName], childColumns) ||
                        !DoAllColumnsExist(dataSet.Tables[principalTableName], parentColumns))
                    {
                        continue;
                    }

                    string relationName = ResolveRelationName(fk, principalTableName, dependentTableName, parentColumns, childColumns);

                    if (RelationAlreadyExists(dataSet, principalTableName, dependentTableName, parentColumns, childColumns))
                    {
                        continue;
                    }

                    IDataRelation relation = new DataRelation();
                    relation.RelationName = relationName;
                    relation.ParentTableName = principalTableName;
                    relation.ChildTableName = dependentTableName;
                    relation.ParentColumnNames = parentColumns;
                    relation.ChildColumnNames = childColumns;

                    dataSet.Relations.Add(relation);
                }
            }
        }

        private static string ResolveRelationName(IForeignKey fk, string parentTable, string childTable, List<string> parentColumns, List<string> childColumns)
        {
            string? constraintName = null;

            try
            {
                constraintName = fk.GetConstraintName();
            }
            catch
            {
                // ignore
            }

            if (!string.IsNullOrWhiteSpace(constraintName))
            {
                return constraintName;
            }

            string parentPart = string.Join("_", parentColumns);
            string childPart = string.Join("_", childColumns);

            return parentTable + "_" + childTable + "_" + parentPart + "_to_" + childPart;
        }

        private static string? ResolveTableName(IEntityType entityType)
        {
            string? tableName = TryGetRelationalTableName(entityType);
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                return tableName;
            }

            if (entityType.ClrType != null)
            {
                return entityType.ClrType.Name;
            }

            return entityType.Name;
        }

        private static List<string> ResolveColumnNames(IEntityType entityType, string tableName, IReadOnlyList<IProperty> properties)
        {
            List<string> result = new List<string>();

            StoreObjectIdentifier? storeObject = TryGetStoreObjectIdentifier(entityType);

            foreach (IProperty property in properties)
            {
                string columnName = GetRelationalColumnNameOrFallback(property, storeObject);
                if (!string.IsNullOrWhiteSpace(columnName))
                {
                    result.Add(columnName);
                }
            }

            return result;
        }

        private static StoreObjectIdentifier? TryGetStoreObjectIdentifier(IEntityType entityType)
        {
            try
            {
                string? tableName = entityType.GetTableName();
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    return null;
                }

                string? schema = null;
                try
                {
                    schema = entityType.GetSchema();
                }
                catch
                {
                    schema = null;
                }

                return StoreObjectIdentifier.Table(tableName, schema);
            }
            catch
            {
                return null;
            }
        }

        private static string GetRelationalColumnNameOrFallback(IProperty property, StoreObjectIdentifier? storeObject)
        {
            if (property == null)
            {
                return string.Empty;
            }

            if (storeObject.HasValue)
            {
                try
                {
                    string? col = property.GetColumnName(storeObject.Value);
                    if (!string.IsNullOrWhiteSpace(col))
                    {
                        return col;
                    }
                }
                catch
                {
                    // ignore
                }
            }

            // Fallback: property name (works for InMemory and non-relational providers).
            return property.Name;
        }

        private static bool DoAllColumnsExist(IDataTable table, List<string> columns)
        {
            if (table == null || columns == null || columns.Count == 0)
            {
                return false;
            }

            foreach (string column in columns)
            {
                if (!table.ContainsColumn(column))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool RelationAlreadyExists(IDataSet dataSet, string parentTable, string childTable, List<string> parentColumns, List<string> childColumns)
        {
            foreach (IDataRelation existing in dataSet.Relations)
            {
                if (!string.Equals(existing.ParentTableName, parentTable, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.Equals(existing.ChildTableName, childTable, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!AreSameColumns(existing.ParentColumnNames, parentColumns))
                {
                    continue;
                }

                if (!AreSameColumns(existing.ChildColumnNames, childColumns))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static bool AreSameColumns(IReadOnlyList<string> a, IReadOnlyList<string> b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!string.Equals(a[i], b[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static string? TryGetRelationalTableName(IEntityType entityType)
        {
            try
            {
                return entityType.GetTableName();
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }
}
