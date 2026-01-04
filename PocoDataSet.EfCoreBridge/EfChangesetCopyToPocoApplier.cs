using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

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
        public static void ApplyTable<TEntity>(
            DbContext dbContext,
            DbSet<TEntity> dbSet,
            IDataTable changesetTable)
            where TEntity : class, new()
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
            if (dbSet == null) throw new ArgumentNullException(nameof(dbSet));
            if (changesetTable == null) throw new ArgumentNullException(nameof(changesetTable));

            if (changesetTable.PrimaryKeys == null || changesetTable.PrimaryKeys.Count == 0)
            {
                throw new InvalidOperationException("EF bridge: changesetTable.PrimaryKeys must be set (single or composite key).");
            }

            Type entityType = typeof(TEntity);
            PropertyInfo[] keyProperties = ResolveKeyProperties(entityType, changesetTable.PrimaryKeys);

            for (int i = 0; i < changesetTable.Rows.Count; i++)
            {
                IDataRow row = changesetTable.Rows[i];

                if (row.DataRowState == DataRowState.Added)
                {
                    TEntity entity = new TEntity();
                    row.CopyToPoco(entity);
                    dbSet.Add(entity);
                    continue;
                }

                object[] keyValues = BuildKeyValues(row, changesetTable.PrimaryKeys);

                if (row.DataRowState == DataRowState.Modified)
                {
                    TEntity? tracked = dbSet.Find(keyValues);
                    if (tracked == null)
                    {
                        tracked = new TEntity();
                        SetKeysOnEntity(tracked, keyProperties, keyValues);
                        dbSet.Attach(tracked);
                    }

                    row.CopyToPoco(tracked);
                    continue;
                }

                if (row.DataRowState == DataRowState.Deleted)
                {
                    TEntity? tracked = dbSet.Find(keyValues);

                    // Idempotent delete: if the entity does not exist in the store, do nothing.
                    // This avoids provider-specific concurrency exceptions (e.g., EF InMemory).
                    if (tracked == null)
                    {
                        continue;
                    }

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
    }
}
