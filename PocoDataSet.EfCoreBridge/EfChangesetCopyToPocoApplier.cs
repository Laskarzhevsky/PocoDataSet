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

        private enum ApplyTableMode
        {
            All,
            NonDeletesOnly,
            DeletesOnly
        }


        public static void ApplyTable<TEntity>(
            DbContext dbContext,
            DbSet<TEntity> dbSet,
            IDataTable changesetTable)
            where TEntity : class, new()
        {
            ApplyTableInternal(dbContext, dbSet, changesetTable, ApplyTableMode.All);
        }

        /// <summary>
        /// Applies only Added and Modified rows (skips Deleted rows).
        /// </summary>
        internal static void ApplyTableNonDeletesOnly<TEntity>(
            DbContext dbContext,
            DbSet<TEntity> dbSet,
            IDataTable changesetTable)
            where TEntity : class, new()
        {
            ApplyTableInternal(dbContext, dbSet, changesetTable, ApplyTableMode.NonDeletesOnly);
        }

        /// <summary>
        /// Applies only Deleted rows (skips Added/Modified rows).
        /// </summary>
        internal static void ApplyTableDeletesOnly<TEntity>(
            DbContext dbContext,
            DbSet<TEntity> dbSet,
            IDataTable changesetTable)
            where TEntity : class, new()
        {
            ApplyTableInternal(dbContext, dbSet, changesetTable, ApplyTableMode.DeletesOnly);
        }

        private static void ApplyTableInternal<TEntity>(
            DbContext dbContext,
            DbSet<TEntity> dbSet,
            IDataTable changesetTable,
            ApplyTableMode mode)
            where TEntity : class, new()
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));
            if (dbSet == null)
                throw new ArgumentNullException(nameof(dbSet));
            if (changesetTable == null)
                throw new ArgumentNullException(nameof(changesetTable));

            if (changesetTable.PrimaryKeys == null || changesetTable.PrimaryKeys.Count == 0)
            {
                throw new InvalidOperationException("EF bridge: changesetTable.PrimaryKeys must be set (single or composite key).");
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
                IDataRow row = changesetTable.Rows[i];

                if (mode != ApplyTableMode.DeletesOnly && row.DataRowState == DataRowState.Added)
                {
                    TEntity entity = new TEntity();
                    row.CopyToPoco(entity);
                    dbSet.Add(entity);
                    continue;
                }

                object[] keyValues = BuildKeyValues(row, changesetTable.PrimaryKeys);

                if (mode != ApplyTableMode.DeletesOnly && row.DataRowState == DataRowState.Modified)
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
                    ApplyFloatingPatchToTrackedEntity(dbContext, tracked, row, keyPropertyNames, concurrencyPropertyNames);
                    continue;
                }

                if (mode != ApplyTableMode.NonDeletesOnly && row.DataRowState == DataRowState.Deleted)
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
                    ApplyConcurrencyTokenOriginalValues(dbContext, tracked, row, concurrencyPropertyNames);

                    dbSet.Remove(tracked);
                    continue;
                }
            }

        }


        public static void ApplyTableAndSave<TEntity>(
            DbContext dbContext,
            DbSet<TEntity> dbSet,
            IDataTable changesetTable)
            where TEntity : class, new()
        {
            ApplyTable(dbContext, dbSet, changesetTable);
            dbContext.SaveChanges();
        }

        public static async Task ApplyTableAndSaveAsync<TEntity>(
            DbContext dbContext,
            DbSet<TEntity> dbSet,
            IDataTable changesetTable,
            CancellationToken cancellationToken = default)
            where TEntity : class, new()
        {
            ApplyTable(dbContext, dbSet, changesetTable);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private static PropertyInfo[] ResolveKeyProperties(Type entityType, System.Collections.Generic.List<string> primaryKeys)
        {
            PropertyInfo[] props = new PropertyInfo[primaryKeys.Count];

            for (int i = 0; i < primaryKeys.Count; i++)
            {
                string keyName = primaryKeys[i];
                props[i] = EfKeyReflection.GetRequiredPropertyIgnoreCase(entityType, keyName);
            }

            return props;
        }

        private static object[] BuildKeyValues(IDataRow row, System.Collections.Generic.List<string> primaryKeys)
        {
            object[] values = new object[primaryKeys.Count];

            for (int i = 0; i < primaryKeys.Count; i++)
            {
                string keyName = primaryKeys[i];

                object? v;
                bool found = row.TryGetValue(keyName, out v);

                if (!found || v == null || v == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "EF bridge: primary key value is missing or null for column '" + keyName + "'.");
                }

                values[i] = v;
            }

            return values;
        }

        private static void SetKeysOnEntity<TEntity>(
            TEntity entity,
            PropertyInfo[] keyProperties,
            object[] keyValues)
        {
            for (int i = 0; i < keyProperties.Length; i++)
            {
                EfKeyReflection.TrySetKey(keyProperties[i], entity!, keyValues[i]);
            }
        }

        private static HashSet<string> BuildKeyPropertyNameSet(PropertyInfo[] keyProperties)
        {
            HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < keyProperties.Length; i++)
            {
                result.Add(keyProperties[i].Name);
            }

            return result;
        }

        private static HashSet<string> GetConcurrencyPropertyNames<TEntity>(DbContext dbContext)
            where TEntity : class
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

        private static void ApplyConcurrencyTokenOriginalValues<TEntity>(
            DbContext dbContext,
            TEntity tracked,
            IDataRow patchRow,
            HashSet<string> concurrencyPropertyNames)
            where TEntity : class
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

        private static void ApplyFloatingPatchToTrackedEntity<TEntity>(
            DbContext dbContext,
            TEntity tracked,
            IDataRow patchRow,
            HashSet<string> keyPropertyNames,
            HashSet<string> concurrencyPropertyNames)
            where TEntity : class
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

        private static object? NormalizeDbNull(object? value)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            return value;
        }

        private static bool TryGetValueIgnoreCase(IDataRow? row, string columnName, out object? value)
        {
            if (row == null)
            {
                value = null;
                return false;
            }

            if (row.TryGetValue(columnName, out value))
            {
                return true;
            }

            string? existingKey;
            if (row.TryGetFieldKeyByColumnName(columnName, out existingKey))
            {
                return row.TryGetValue(existingKey!, out value);
            }

            value = null;
            return false;
        }
    }
}