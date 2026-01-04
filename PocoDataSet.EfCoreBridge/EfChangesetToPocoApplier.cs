using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Applies PocoDataSet changesets to EF Core using detached entities created via ToPoco.
    /// </summary>
    public static class EfChangesetToPocoApplier
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

            for (int i = 0; i < changesetTable.Rows.Count; i++)
            {
                IDataRow row = changesetTable.Rows[i];

                if (row.DataRowState == DataRowState.Added)
                {
                    TEntity entity = row.ToPoco<TEntity>();
                    dbSet.Add(entity);
                }
                else if (row.DataRowState == DataRowState.Modified)
                {
                    TEntity entity = row.ToPoco<TEntity>();
                    dbSet.Update(entity);
                }
                else if (row.DataRowState == DataRowState.Deleted)
                {
                    TEntity entity = row.ToPoco<TEntity>();
                    dbSet.Remove(entity);
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
    }
}
