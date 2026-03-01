using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using PocoDataSet.Data;
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
        /// Applies changeset, saves, and returns a minimal post-save response dataset.
        /// The response dataset is suitable for <c>DoPostSaveMerge</c> on the UI side.
        /// By default, the response contains only keys, __ClientKey (when present), and EF Core concurrency tokens.
        /// </summary>
        /// <param name="dbContext">Db context</param>
        /// <param name="changeset">Changeset to apply</param>
        /// <returns>Post-save response dataset (delta payload)</returns>
        public static IDataSet ApplyChangesetAndSaveReturningPostSaveResponse(DbContext dbContext, IDataSet changeset)
        {
            return ApplyChangesetAndSaveReturningPostSaveResponse(dbContext, changeset, (RelationValidationOptions?)null);
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
        /// Applies changeset, saves, and returns a minimal post-save response dataset.
        /// </summary>
        /// <param name="dbContext">Db context</param>
        /// <param name="changeset">Changeset to apply</param>
        /// <param name="relationOptions">Relation options</param>
        /// <returns>Post-save response dataset (delta payload)</returns>
        public static IDataSet ApplyChangesetAndSaveReturningPostSaveResponse(DbContext dbContext, IDataSet changeset, RelationValidationOptions? relationOptions)
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
            return ApplyChangesetAndSaveReturningPostSaveResponse(dbContext, changeset, resolver, effectiveOptions);
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
        /// Applies changeset, saves, and returns a minimal post-save response dataset asynchronously.
        /// </summary>
        public static Task<IDataSet> ApplyChangesetAndSaveReturningPostSaveResponseAsync(DbContext dbContext, IDataSet changeset, CancellationToken cancellationToken)
        {
            return ApplyChangesetAndSaveReturningPostSaveResponseAsync(dbContext, changeset, (RelationValidationOptions?)null, cancellationToken);
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

        /// <summary>
        /// Applies changeset, saves, and returns a minimal post-save response dataset asynchronously.
        /// </summary>
        public static async Task<IDataSet> ApplyChangesetAndSaveReturningPostSaveResponseAsync(DbContext dbContext, IDataSet changeset, RelationValidationOptions? relationOptions, CancellationToken cancellationToken)
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
            return await ApplyChangesetAndSaveReturningPostSaveResponseAsync(dbContext, changeset, resolver, effectiveOptions, cancellationToken).ConfigureAwait(false);
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
        /// Applies changeset, saves, and returns a minimal post-save response dataset.
        /// </summary>
        public static IDataSet ApplyChangesetAndSaveReturningPostSaveResponse(DbContext dbContext, IDataSet changeset, IEntityTypeResolver entityTypeResolver, RelationValidationOptions? relationOptions = null)
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

            changeset.EnsureRelationsValid(options);

            List<AppliedEntityRow> affected = ApplyChangesetCollect(dbContext, changeset, entityTypeResolver);
            dbContext.SaveChanges();

            return BuildPostSaveResponseDataSet(dbContext, changeset, affected);
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

        /// <summary>
        /// Applies changeset, saves, and returns a minimal post-save response dataset asynchronously.
        /// </summary>
        public static async Task<IDataSet> ApplyChangesetAndSaveReturningPostSaveResponseAsync(DbContext dbContext, IDataSet changeset, IEntityTypeResolver entityTypeResolver, RelationValidationOptions? relationOptions = null, CancellationToken cancellationToken = default)
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

            changeset.EnsureRelationsValid(options);

            List<AppliedEntityRow> affected = ApplyChangesetCollect(dbContext, changeset, entityTypeResolver);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return BuildPostSaveResponseDataSet(dbContext, changeset, affected);
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
        /// Applies changeset (in relation-aware phases) and collects mappings between changeset rows and EF entities.
        /// </summary>
        private static List<AppliedEntityRow> ApplyChangesetCollect(DbContext dbContext, IDataSet changeset, IEntityTypeResolver entityTypeResolver)
        {
            List<AppliedEntityRow> affected = new List<AppliedEntityRow>();

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

                List<AppliedEntityRow> collected = ApplyTableByTypeCollect(dbContext, entityType, table, tableName, false);
                for (int j = 0; j < collected.Count; j++)
                {
                    affected.Add(collected[j]);
                }
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

                List<AppliedEntityRow> collected = ApplyTableByTypeCollect(dbContext, entityType, table, tableName, true);
                for (int j = 0; j < collected.Count; j++)
                {
                    affected.Add(collected[j]);
                }
            }

            return affected;
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
        /// Applies table by type and collects affected entity mappings.
        /// </summary>
        private static List<AppliedEntityRow> ApplyTableByTypeCollect(DbContext dbContext, Type entityType, IDataTable changesetTable, string tableName, bool deletesOnly)
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
                applyMethodName = "ApplyTableDeletesOnlyCollect";
            }
            else
            {
                applyMethodName = "ApplyTableNonDeletesOnlyCollect";
            }

            MethodInfo? applyMethod = typeof(EfChangesetCopyToPocoApplier).GetMethod(applyMethodName, BindingFlags.NonPublic | BindingFlags.Static);
            if (applyMethod == null)
            {
                throw new InvalidOperationException("EF bridge: cannot find '" + applyMethodName + "' method on EfChangesetCopyToPocoApplier.");
            }

            MethodInfo genericApply = applyMethod.MakeGenericMethod(entityType);

            object[] args = new object[4];
            args[0] = dbContext;
            args[1] = dbSetObject;
            args[2] = changesetTable;
            args[3] = tableName;

            object? result = genericApply.Invoke(null, args);
            List<AppliedEntityRow>? list = result as List<AppliedEntityRow>;

            if (list == null)
            {
                return new List<AppliedEntityRow>();
            }

            return list;
        }

        /// <summary>
        /// Builds a minimal post-save response dataset that can be merged via DoPostSaveMerge.
        /// </summary>
        private static IDataSet BuildPostSaveResponseDataSet(DbContext dbContext, IDataSet changeset, List<AppliedEntityRow> affected)
        {
            DataSet response = new DataSet();

            // Build one response table per affected table name.
            Dictionary<string, DataTable> responseTables = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < affected.Count; i++)
            {
                AppliedEntityRow item = affected[i];
                if (item == null)
                {
                    continue;
                }

                string tableName = item.TableName;
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    continue;
                }

                DataTable table;
                if (!responseTables.TryGetValue(tableName, out table!))
                {
                    table = new DataTable();
                    table.TableName = tableName;

                    // Use changeset schema as the simplest source of truth.
                    IDataTable sourceTable = changeset.Tables[tableName];
                    table.AddColumns(CloneColumns(sourceTable.Columns));
                    // Primary keys are derived from column metadata flags (IColumnMetadata.IsPrimaryKey).
                    // DataTable.SetPrimaryKeys is internal; cloning column metadata preserves PK flags.

                    response.AddTable(table);
                    responseTables[tableName] = table;
                }

                // Deleted rows do not need a response payload row.
                // PostSave merge finalizes deletes based on current table state.
                if (item.SourceState == DataRowState.Deleted)
                {
                    continue;
                }

                IFloatingDataRow responseRow = new FloatingDataRow();

                // Echo __ClientKey for correlation when present.
                object? clientKey;
                if (TryGetValueIgnoreCase(item.SourceRow, "__ClientKey", out clientKey))
                {
                    responseRow["__ClientKey"] = clientKey;
                }

                // Always include PK + EF concurrency tokens from the tracked entity instance.
                CopyKeysAndConcurrencyTokens(dbContext, item.Entity, responseRow);

                // Mark state for merge semantics. IDataRow.DataRowState is read-only; use SetDataRowState.
                // Added is important for identity propagation scenarios; Modified is sufficient otherwise.
                if (item.SourceState == DataRowState.Added)
                {
                    responseRow.SetDataRowState(DataRowState.Added);
                }
                else
                {
                    responseRow.SetDataRowState(DataRowState.Modified);
                }

                table.AddRow(responseRow);
            }

            // Keep response schema aligned with EF model for downstream tooling.
            response.ApplyEfModelKeysAndRelations(dbContext);
            return response;
        }

        private static List<IColumnMetadata> CloneColumns(IReadOnlyList<IColumnMetadata> columns)
        {
            List<IColumnMetadata> list = new List<IColumnMetadata>();
            for (int i = 0; i < columns.Count; i++)
            {
                list.Add(columns[i].Clone());
            }

            return list;
        }

        private static void CopyKeysAndConcurrencyTokens(DbContext dbContext, object entity, IFloatingDataRow responseRow)
        {
            if (entity == null)
            {
                return;
            }

            IEntityType? efEntityType = null;
            try
            {
                efEntityType = dbContext.Model.FindEntityType(entity.GetType());
            }
            catch
            {
                efEntityType = null;
            }

            if (efEntityType == null)
            {
                return;
            }

            // Primary keys
            IKey? pk = efEntityType.FindPrimaryKey();
            if (pk != null)
            {
                IReadOnlyList<IProperty> props = pk.Properties;
                for (int i = 0; i < props.Count; i++)
                {
                    string name = props[i].Name;
                    object? value = ReadPropertyValueIgnoreCase(entity, name);
                    if (value != null)
                    {
                        responseRow[name] = value;
                    }
                }
            }

            // Concurrency tokens (e.g., RowVersion)
            foreach (IProperty property in efEntityType.GetProperties())
            {
                if (!property.IsConcurrencyToken)
                {
                    continue;
                }

                string name = property.Name;
                object? value = ReadPropertyValueIgnoreCase(entity, name);
                if (value != null)
                {
                    responseRow[name] = value;
                }
            }
        }

        private static object? ReadPropertyValueIgnoreCase(object entity, string propertyName)
        {
            try
            {
                PropertyInfo? pi = entity.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (pi == null)
                {
                    return null;
                }

                return pi.GetValue(entity);
            }
            catch
            {
                return null;
            }
        }

        private static bool TryGetValueIgnoreCase(IDataRow? dataRow, string columnName, out object? value)
        {
            if (dataRow == null)
            {
                value = null;
                return false;
            }

            if (dataRow.TryGetValue(columnName, out value))
            {
                return true;
            }

            string? existingKey;
            if (dataRow.TryGetFieldKeyByColumnName(columnName, out existingKey))
            {
                return dataRow.TryGetValue(existingKey!, out value);
            }

            value = null;
            return false;
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
