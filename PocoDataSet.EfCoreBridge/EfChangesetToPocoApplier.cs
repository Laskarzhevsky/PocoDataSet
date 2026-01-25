using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Applies PocoDataSet changesets to EF Core.
    ///
    /// IMPORTANT: In the "floating row" model, Modified and Deleted rows are sparse (PATCH payloads).
    /// Therefore, this applier delegates to <see cref="EfChangesetCopyToPocoApplier"/> so that:
    /// - only provided fields are applied
    /// - unknown columns are ignored
    /// - only provided fields are marked modified
    /// </summary>
    public static class EfChangesetToPocoApplier
    {
        #region Public Methods
        /// <summary>
        /// Applies table
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">Db context</param>
        /// <param name="dbSet">Db set</param>
        /// <param name="changesetTable">Changeset table</param>
        public static void ApplyTable<TEntity>(DbContext dbContext, DbSet<TEntity> dbSet, IDataTable changesetTable) where TEntity : class, new()
        {
            EfChangesetCopyToPocoApplier.ApplyTable(dbContext, dbSet, changesetTable);
        }

        /// <summary>
        /// Applies table and saves
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">Db context</param>
        /// <param name="dbSet">Db set</param>
        /// <param name="changesetTable">Changeset table</param>
        public static void ApplyTableAndSave<TEntity>(DbContext dbContext, DbSet<TEntity> dbSet, IDataTable changesetTable) where TEntity : class, new()
        {
            ApplyTable(dbContext, dbSet, changesetTable);
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Applies table and saves asynchronously
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">Db context</param>
        /// <param name="dbSet">Db set</param>
        /// <param name="changesetTable">Changeset table</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task ApplyTableAndSaveAsync<TEntity>(DbContext dbContext, DbSet<TEntity> dbSet, IDataTable changesetTable, CancellationToken cancellationToken = default) where TEntity : class, new()
        {
            ApplyTable(dbContext, dbSet, changesetTable);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        #endregion
    }
}
