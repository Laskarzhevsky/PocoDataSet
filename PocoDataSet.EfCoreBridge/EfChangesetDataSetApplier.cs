using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using PocoDataSet.Extensions;
using PocoDataSet.Extensions.Relations;
using PocoDataSet.IData;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Applies a full PocoDataSet changeset (multiple tables) to EF Core in a single unit of work.
    /// Validates relations before persistence and applies tables in a relation-aware order.
    /// </summary>
    public static class EfChangesetDataSetApplier
    {
        #region Public Methods (no resolver required)
        /// <summary>
        /// Applies changeset and saves
        /// </summary>
        /// <param name="dbContext">Db context</param>
        /// <param name="changeset">Changeset to apply</param>
        public static void ApplyChangesetAndSave(DbContext dbContext, IDataSet changeset)
        {
            ApplyChangesetAndSave(dbContext, changeset, (RelationValidationOptions?)null);
        }

        /// <summary>
        /// Applies changeset and saves
        /// </summary>
        /// <param name="dbContext">Db context</param>
        /// <param name="changeset">Changeset to apply</param>
        /// <param name="relationOptions">Relation options</param>
        public static void ApplyChangesetAndSave(DbContext dbContext, IDataSet changeset, RelationValidationOptions? relationOptions)
        {
            RelationValidationOptions effectiveOptions;
            if (relationOptions == null)
            {
                effectiveOptions = new RelationValidationOptions();
            }
            else
            {
                effectiveOptions = relationOptions;
            }

            EfModelEntityTypeResolver resolver = new EfModelEntityTypeResolver(dbContext);
            ApplyChangesetAndSave(dbContext, changeset, resolver, effectiveOptions);
        }

        /// <summary>
        /// Applies changeset and saves asynchronously
        /// </summary>
        /// <param name="dbContext">Db context</param>
        /// <param name="changeset">Changeset to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static Task ApplyChangesetAndSaveAsync(DbContext dbContext, IDataSet changeset, CancellationToken cancellationToken)
        {
            return ApplyChangesetAndSaveAsync(dbContext, changeset, (RelationValidationOptions?)null, cancellationToken);
        }

        /// <summary>
        /// Applies changeset and saves asynchronously
        /// </summary>
        /// <param name="dbContext">Db context</param>
        /// <param name="changeset">Changeset to apply</param>
        /// <param name="relationOptions">Relation options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static Task ApplyChangesetAndSaveAsync(DbContext dbContext, IDataSet changeset, RelationValidationOptions? relationOptions, CancellationToken cancellationToken)
        {
            RelationValidationOptions effectiveOptions;
            if (relationOptions == null)
            {
                effectiveOptions = new RelationValidationOptions();
            }
            else
            {
                effectiveOptions = relationOptions;
            }

            EfModelEntityTypeResolver resolver = new EfModelEntityTypeResolver(dbContext);
            return ApplyChangesetAndSaveAsync(dbContext, changeset, resolver, effectiveOptions, cancellationToken);
        }
        #endregion

        #region Public Methods (resolver overloads)
        /// <summary>
        /// Applies changeset and saves
        /// </summary>
        /// <param name="dbContext">Db context</param>
        /// <param name="changeset">Changeset to apply</param>
        /// <param name="entityTypeResolver">Entity type resolver</param>
        /// <param name="relationOptions">Relation options</param>
        public static void ApplyChangesetAndSave(DbContext dbContext, IDataSet changeset, IEntityTypeResolver entityTypeResolver, RelationValidationOptions? relationOptions = null)
        {
            RelationValidationOptions options;
            if (relationOptions == null)
            {
                options = new RelationValidationOptions();
            }
            else
            {
                options = relationOptions;
            }

            // Validate relations before touching EF.
            changeset.EnsureRelationsValid(options);

            ApplyChangeset(dbContext, changeset, entityTypeResolver);
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Applies changeset and saves asyncronously
        /// </summary>
        /// <param name="dbContext">Db context</param>
        /// <param name="changeset">Changeset to apply</param>
        /// <param name="entityTypeResolver">Entity type resolver</param>
        /// <param name="relationOptions">Relation options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task ApplyChangesetAndSaveAsync(DbContext dbContext, IDataSet changeset, IEntityTypeResolver entityTypeResolver, RelationValidationOptions? relationOptions = null, CancellationToken cancellationToken = default)
        {
            RelationValidationOptions options;
            if (relationOptions == null)
            {
                options = new RelationValidationOptions();
            }
            else
            {
                options = relationOptions;
            }

            // Validate relations before touching EF.
            changeset.EnsureRelationsValid(options);

            ApplyChangeset(dbContext, changeset, entityTypeResolver);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Applies changeset
        /// </summary>
        /// <param name="dbContext">Db context</param>
        /// <param name="changeset">Changeset to apply</param>
        /// <param name="entityTypeResolver">Entity type resolver</param>
        private static void ApplyChangeset(DbContext dbContext, IDataSet changeset, IEntityTypeResolver entityTypeResolver)
        {
            List<string> tableNames = new List<string>();
            foreach (KeyValuePair<string, IDataTable> kvp in changeset.Tables)
            {
                if (kvp.Value == null)
                {
                    continue;
                }

                string name = kvp.Key;

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                tableNames.Add(name);
            }

            List<string> ordered = RelationTableSorter.SortTablesByRelations(changeset, tableNames);

            // Phase 1: Added/Modified in parent -> child order
            for (int i = 0; i < ordered.Count; i++)
            {
                string tableName = ordered[i];
                IDataTable table = changeset.Tables[tableName];

                Type entityType;

                if (!entityTypeResolver.TryResolveEntityType(tableName, out entityType))
                {
                    throw CreateMissingMappingException(tableName);
                }

                ApplyTableByType(dbContext, entityType, table, false);
            }

            // Phase 2: Deleted in child -> parent order
            for (int i = ordered.Count - 1; i >= 0; i--)
            {
                string tableName = ordered[i];
                IDataTable table = changeset.Tables[tableName];

                Type entityType;

                if (!entityTypeResolver.TryResolveEntityType(tableName, out entityType))
                {
                    throw CreateMissingMappingException(tableName);
                }

                ApplyTableByType(dbContext, entityType, table, true);
            }
        }

        /// <summary>
        /// Applies table by type
        /// </summary>
        /// <param name="dbContext">Db context</param>
        /// <param name="entityType">Entity type</param>
        /// <param name="changesetTable">Changeset table</param>
        /// <param name="deletesOnly">Flag indicating whether deletes only need to be applied</param>
        private static void ApplyTableByType(DbContext dbContext, Type entityType, IDataTable changesetTable, bool deletesOnly)
        {
            MethodInfo? setMethod = typeof(DbContext).GetMethod("Set", Array.Empty<Type>());
            if (setMethod == null)
            {
                throw new InvalidOperationException("EF bridge: cannot find DbContext.Set<TEntity>() method.");
            }

            MethodInfo genericSetMethod = setMethod.MakeGenericMethod(entityType);
            object? dbSetObject = genericSetMethod.Invoke(dbContext, null);
            if (dbSetObject == null)
            {
                throw new InvalidOperationException("EF bridge: DbContext.Set<TEntity>() returned null for type '" + entityType.FullName + "'.");
            }

            string applyMethodName;
            if (deletesOnly)
            {
                applyMethodName = "ApplyTableDeletesOnly";
            }
            else
            {
                applyMethodName = "ApplyTableNonDeletesOnly";
            }

            MethodInfo? applyMethod = typeof(EfChangesetCopyToPocoApplier).GetMethod(applyMethodName, BindingFlags.NonPublic | BindingFlags.Static);
            if (applyMethod == null)
            {
                throw new InvalidOperationException("EF bridge: cannot find '" + applyMethodName + "' method on EfChangesetCopyToPocoApplier.");
            }

            MethodInfo genericApply = applyMethod.MakeGenericMethod(entityType);

            object[] args = new object[3];
            args[0] = dbContext;
            args[1] = dbSetObject;
            args[2] = changesetTable;

            genericApply.Invoke(null, args);
        }

        /// <summary>
        /// Creates missing mapping exception
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Created missing mapping exception</returns>
        private static InvalidOperationException CreateMissingMappingException(string tableName)
        {
            return new InvalidOperationException(
                "EF bridge: no entity type mapping for table '" + tableName + "'. " +
                "Provide IEntityTypeResolver mapping, or annotate the entity with [Table(\"" + tableName + "\")] " +
                "or [ChangesetTable(\"" + tableName + "\")] (if EF table name differs).");
        }
        #endregion
    }
}
