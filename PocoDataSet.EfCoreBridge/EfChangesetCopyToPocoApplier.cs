using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Applies PocoDataSet changesets to EF Core using tracked entity instances updated via CopyToPoco.
    /// Supports single and composite primary keys by using <see cref="IDataTable.PrimaryKeys"/>.
    /// </summary>
    public static class EfChangesetCopyToPocoApplier
    {
        #region Public Methods
        /// <summary>
        /// Applies the changes from the specified data table to the given DbSet within the provided DbContext.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity in the DbSet. Must be a reference type with a parameterless constructor.</typeparam>
        /// <param name="dbContext">The DbContext instance that manages the entity set and tracks changes.</param>
        /// <param name="dbSet">The DbSet representing the collection of entities to which the changes will be applied.</param>
        /// <param name="changesetTable">An IDataTable containing the set of changes to apply to the DbSet. The table should represent insert,
        /// update, or delete operations for entities of type TEntity.</param>
        public static void ApplyTable<TEntity>(DbContext dbContext, DbSet<TEntity> dbSet, IDataTable changesetTable) where TEntity : class, new()
        {
            ApplyTableInternal(dbContext, dbSet, changesetTable, ApplyTableMode.All);
        }

        /// <summary>
        /// Applies changes from the specified data table to the given entity set and saves the changes to the database.
        /// </summary>
        /// <remarks>This method applies all changes from the provided data table to the specified entity
        /// set and immediately persists those changes to the database by calling SaveChanges on the context. Ensure
        /// that the data table accurately represents the intended modifications to avoid unintended data updates.
        /// </remarks>
        /// <typeparam name="TEntity">The type of the entity in the DbSet. Must be a reference type with a parameterless constructor.</typeparam>
        /// <param name="dbContext">The database context used to track and persist changes.</param>
        /// <param name="dbSet">The entity set to which the changes will be applied.</param>
        /// <param name="changesetTable">A data table containing the changes to apply to the entity set. The table should be structured to match the
        /// entity type.</param>
        public static void ApplyTableAndSave<TEntity>(DbContext dbContext, DbSet<TEntity> dbSet, IDataTable changesetTable) where TEntity : class, new()
        {
            ApplyTable(dbContext, dbSet, changesetTable);
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Applies the specified changeset table to the given DbSet and saves all changes to the database asynchronously.
        /// </summary>
        /// <remarks>This method first applies the changes described in the changeset table to the
        /// specified DbSet, then saves all pending changes in the DbContext to the database. If the cancellation token
        /// is triggered before the save operation completes, the operation is canceled.</remarks>
        /// <typeparam name="TEntity">The type of the entity in the DbSet. Must be a reference type with a parameterless constructor.</typeparam>
        /// <param name="dbContext">The DbContext instance used to track changes and persist them to the database. Cannot be null.</param>
        /// <param name="dbSet">The DbSet representing the collection of entities to which the changeset will be applied. Cannot be null.</param>
        /// <param name="changesetTable">The changeset table containing the data modifications to apply to the DbSet. Cannot be null.</param>
        /// <param name="cancellationToken">A CancellationToken that can be used to cancel the asynchronous save operation. Optional.</param>
        /// <returns>A task that represents the asynchronous operation. The task completes when all changes have been saved to
        /// the database.</returns>
        public static async Task ApplyTableAndSaveAsync<TEntity>(DbContext dbContext, DbSet<TEntity> dbSet, IDataTable changesetTable, CancellationToken cancellationToken = default) where TEntity : class, new()
        {
            ApplyTable(dbContext, dbSet, changesetTable);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Applies only delete rows from the specified changeset table to the given entity set in the database context.
        /// </summary>
        /// <remarks>This method processes only delete operations; insertions and updates in the changeset
        /// table are ignored. Use this method when you want to synchronize deletions from an external data source
        /// without affecting existing or new records.</remarks>
        /// <typeparam name="TEntity">The type of the entity in the DbSet. Must be a reference type with a parameterless constructor.</typeparam>
        /// <param name="dbContext">The database context to which the delete operations will be applied.</param>
        /// <param name="dbSet">The entity set representing the table from which entities will be deleted.</param>
        /// <param name="changesetTable">A data table containing the changeset with rows to be deleted from the entity set.</param>
        internal static void ApplyTableDeletesOnly<TEntity>(DbContext dbContext, DbSet<TEntity> dbSet, IDataTable changesetTable) where TEntity : class, new()
        {
            ApplyTableInternal(dbContext, dbSet, changesetTable, ApplyTableMode.DeletesOnly);
        }

        /// <summary>
        /// Applies only Added and Modified rows (skips Deleted rows) from the specified data table to the given DbSet within the provided DbContext.
        /// </summary>
        /// <remarks>This method processes only insert and update operations from the changeset table.
        /// Delete operations are ignored.</remarks>
        /// <typeparam name="TEntity">The type of the entity in the DbSet. Must be a reference type with a parameterless constructor.</typeparam>
        /// <param name="dbContext">The DbContext instance that manages the entity objects and database connection.</param>
        /// <param name="dbSet">The DbSet representing the collection of entities to which changes will be applied.</param>
        /// <param name="changesetTable">The data table containing the changes to apply. Only non-delete operations are processed.</param>
        internal static void ApplyTableNonDeletesOnly<TEntity>(DbContext dbContext, DbSet<TEntity> dbSet, IDataTable changesetTable) where TEntity : class, new()
        {
            ApplyTableInternal(dbContext, dbSet, changesetTable, ApplyTableMode.NonDeletesOnly);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Applies concurrency token original values
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">Db context</param>
        /// <param name="tracked">Entity instance</param>
        /// <param name="patchRow">Patch row</param>
        /// <param name="concurrencyPropertyNames">Concurrency property names</param>
        private static void ApplyConcurrencyTokenOriginalValues<TEntity>(DbContext dbContext, TEntity tracked, IDataRow patchRow, HashSet<string> concurrencyPropertyNames) where TEntity : class
        {
            EntityEntry<TEntity> entry = dbContext.Entry(tracked);

            foreach (string concurrencyName in concurrencyPropertyNames)
            {
                object? raw;
                if (!TryGetValueIgnoreCase(patchRow, concurrencyName, out raw))
                {
                    continue;
                }

                object? original = NormalizeDbNull(raw);

                try
                {
                    entry.Property(concurrencyName).OriginalValue = original;
                    entry.Property(concurrencyName).IsModified = false;
                }
                catch
                {
                    // ignore if EF cannot bind the property by name (e.g., shadow properties)
                }
            }
        }

        /// <summary>
        /// Applies floating patch to tracked entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">Db context</param>
        /// <param name="tracked">Entity instance</param>
        /// <param name="patchRow">Patch row</param>
        /// <param name="keyPropertyNames">Key property names</param>
        /// <param name="concurrencyPropertyNames">Concurrency property names</param>
        private static void ApplyFloatingPatchToTrackedEntity<TEntity>(DbContext dbContext, TEntity tracked, IDataRow patchRow, HashSet<string> keyPropertyNames, HashSet<string> concurrencyPropertyNames) where TEntity : class
        {
            // 1) Copy only provided values.
            patchRow.CopyToPoco(tracked);

            EntityEntry<TEntity> entry = dbContext.Entry(tracked);

            // 2) If concurrency token is provided, apply it as OriginalValue.
            //    Do not treat it as a modified value.
            foreach (string concurrencyName in concurrencyPropertyNames)
            {
                object? raw;
                if (!TryGetValueIgnoreCase(patchRow, concurrencyName, out raw))
                {
                    continue;
                }

                object? original = NormalizeDbNull(raw);

                try
                {
                    entry.Property(concurrencyName).OriginalValue = original;
                    entry.Property(concurrencyName).IsModified = false;
                }
                catch
                {
                    // ignore if EF cannot bind the property by name (e.g., shadow properties)
                }
            }

            // 3) Explicitly mark provided fields (excluding keys and concurrency tokens) as modified.
            //    This ensures the floating row behaves like a PATCH payload.
            PropertyInfo[] properties = tracked.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                if (!property.CanWrite)
                {
                    continue;
                }

                string propertyName = property.Name;

                if (keyPropertyNames.Contains(propertyName))
                {
                    continue;
                }

                if (concurrencyPropertyNames.Contains(propertyName))
                {
                    continue;
                }

                object? provided;
                if (!TryGetValueIgnoreCase(patchRow, propertyName, out provided))
                {
                    continue;
                }

                try
                {
                    // Mark as modified even if the value matches current state.
                    // Patch payload semantics: "provided" means "apply".
                    entry.Property(propertyName).IsModified = true;
                }
                catch
                {
                    // Ignore properties EF doesn't map (or shadow properties).
                }
            }
        }

        /// <summary>
        /// Applies changes from a data table to the specified DbSet within the given DbContext, performing insert,
        /// update, or delete operations according to the provided mode.
        /// </summary>
        /// <remarks>This method processes each row in the changeset table and applies the corresponding
        /// operation to the DbSet based on the row's state and the specified mode. Inserted and modified rows are added
        /// or updated, while deleted rows are removed. Concurrency tokens, if present, are handled to ensure correct
        /// update and delete predicates. The method is idempotent for deletes: attempting to delete a non-existent
        /// entity has no effect.</remarks>
        /// <typeparam name="TEntity">The type of the entity represented by the DbSet. Must be a reference type with a parameterless constructor.</typeparam>
        /// <param name="dbContext">The DbContext instance used to track and persist entity changes.</param>
        /// <param name="dbSet">The DbSet representing the collection of entities to which changes will be applied.</param>
        /// <param name="changesetTable">The data table containing the set of changes to apply. Must define one or more primary key columns.</param>
        /// <param name="mode">Specifies which types of changes (inserts, updates, deletes) to apply from the data table.</param>
        /// <exception cref="InvalidOperationException">Thrown if the changeset table does not define a primary key.</exception>
        private static void ApplyTableInternal<TEntity>(DbContext dbContext, DbSet<TEntity> dbSet, IDataTable changesetTable, ApplyTableMode mode) where TEntity : class, new()
        {
            if (changesetTable.PrimaryKeys == null || changesetTable.PrimaryKeys.Count == 0)
            {
                throw new InvalidOperationException("Changeset table must have single or composite primary key");
            }

            Type entityType = typeof(TEntity);
            PropertyInfo[] keyProperties = ResolveKeyProperties(entityType, changesetTable.PrimaryKeys);

            // Concurrency token properties (e.g., RowVersion/Timestamp).
            // If a concurrency token is provided in the floating row, we set it as OriginalValue
            // so EF Core generates an appropriate WHERE predicate.
            HashSet<string> concurrencyPropertyNames = GetConcurrencyPropertyNames<TEntity>(dbContext);
            HashSet<string> keyPropertyNames = BuildKeyPropertyNameSet(keyProperties);

            for (int i = 0; i < changesetTable.Rows.Count; i++)
            {
                IDataRow dataRow = changesetTable.Rows[i];
                if (mode != ApplyTableMode.DeletesOnly && dataRow.DataRowState == DataRowState.Added)
                {
                    TEntity entity = new TEntity();
                    dataRow.CopyToPoco(entity);
                    dbSet.Add(entity);
                    continue;
                }

                object[] keyValues = BuildKeyValues(dataRow, changesetTable.PrimaryKeys);
                if (mode != ApplyTableMode.DeletesOnly && dataRow.DataRowState == DataRowState.Modified)
                {
                    TEntity? tracked = dbSet.Find(keyValues);
                    if (tracked == null)
                    {
                        tracked = new TEntity();
                        SetKeysOnEntity(tracked, keyProperties, keyValues);
                        dbSet.Attach(tracked);
                    }

                    // Apply floating row as a PATCH:
                    // - Only provided fields are copied.
                    // - Unknown columns are ignored.
                    // - Provided fields are explicitly marked modified.
                    ApplyFloatingPatchToTrackedEntity(dbContext, tracked, dataRow, keyPropertyNames, concurrencyPropertyNames);
                    continue;
                }

                if (mode != ApplyTableMode.NonDeletesOnly && dataRow.DataRowState == DataRowState.Deleted)
                {
                    TEntity? tracked = dbSet.Find(keyValues);

                    // Idempotent delete: if the entity does not exist in the store, do nothing.
                    // This avoids provider-specific concurrency exceptions (e.g., EF InMemory).
                    if (tracked == null)
                    {
                        continue;
                    }

                    // If the floating delete row provides a concurrency token (e.g., RowVersion),
                    // apply it as OriginalValue so EF Core includes it in the DELETE predicate.
                    ApplyConcurrencyTokenOriginalValues(dbContext, tracked, dataRow, concurrencyPropertyNames);

                    dbSet.Remove(tracked);
                    continue;
                }
            }
        }

        /// <summary>
        /// Builds key values
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <param name="primaryKeys">Primary keys</param>
        /// <returns>Built key values</returns>
        private static object[] BuildKeyValues(IDataRow dataRow, System.Collections.Generic.IReadOnlyList<string> primaryKeys)
        {
            object[] keyValues = new object[primaryKeys.Count];
            for (int i = 0; i < primaryKeys.Count; i++)
            {
                string keyName = primaryKeys[i];

                object? keyValue;
                bool found = dataRow.TryGetValue(keyName, out keyValue);
                if (!found || keyValue == null || keyValue == DBNull.Value)
                {
                    throw new InvalidOperationException($"Primary key value is missing or null for column {keyName}");
                }

                keyValues[i] = keyValue;
            }

            return keyValues;
        }

        /// <summary>
        /// Builds key / property name set
        /// </summary>
        /// <param name="keyProperties">Key properties</param>
        /// <returns>Built key / property name set</returns>
        private static HashSet<string> BuildKeyPropertyNameSet(PropertyInfo[] keyProperties)
        {
            HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < keyProperties.Length; i++)
            {
                result.Add(keyProperties[i].Name);
            }

            return result;
        }

        /// <summary>
        /// Resolves key properties
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <param name="primaryKeys">Primary keys</param>
        /// <returns>Resolved key properties</returns>
        private static PropertyInfo[] ResolveKeyProperties(Type entityType, System.Collections.Generic.IReadOnlyList<string> primaryKeys)
        {
            PropertyInfo[] props = new PropertyInfo[primaryKeys.Count];
            for (int i = 0; i < primaryKeys.Count; i++)
            {
                string keyName = primaryKeys[i];
                props[i] = EfKeyReflection.GetRequiredPropertyIgnoreCase(entityType, keyName);
            }

            return props;
        }

        /// <summary>
        /// Gets concurrency property names
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">Db context</param>
        /// <returns>Concurrency property names</returns>
        private static HashSet<string> GetConcurrencyPropertyNames<TEntity>(DbContext dbContext) where TEntity : class
        {
            HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                Microsoft.EntityFrameworkCore.Metadata.IEntityType? efEntityType = dbContext.Model.FindEntityType(typeof(TEntity));
                if (efEntityType == null)
                {
                    return result;
                }

                foreach (Microsoft.EntityFrameworkCore.Metadata.IProperty property in efEntityType.GetProperties())
                {
                    if (property.IsConcurrencyToken)
                    {
                        result.Add(property.Name);
                    }
                }
            }
            catch
            {
                // swallow - concurrency tokens are optional
            }

            return result;
        }

        /// <summary>
        /// Normalizes DbNull
        /// </summary>
        /// <param name="value">Calue to normilize</param>
        /// <returns>Normalized DbNull</returns>
        private static object? NormalizeDbNull(object? value)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            return value;
        }

        /// <summary>
        /// Sets keys on entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity instance</param>
        /// <param name="keyProperties">Key properties</param>
        /// <param name="keyValues">Key values</param>
        private static void SetKeysOnEntity<TEntity>(TEntity entity, PropertyInfo[] keyProperties, object[] keyValues)
        {
            for (int i = 0; i < keyProperties.Length; i++)
            {
                EfKeyReflection.TrySetKey(keyProperties[i], entity!, keyValues[i]);
            }
        }

        /// <summary>
        /// Tries to get field value ignoring case of column name
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value"></param>
        /// <returns>Field value</returns>
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
        #endregion
    }
}