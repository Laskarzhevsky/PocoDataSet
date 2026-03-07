using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using PocoDataSet.ObjectData;

namespace PocoDataSet.ObjectEfCoreBridge
{
    /// <summary>
    /// Developer-experience facade for loading typed EF Core query results into ObjectDataSet.
    /// </summary>
    public static class EfCoreObjectDataAdapter
    {
        #region Public Methods
        /// <summary>
        /// Loads data into a new object data set.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">DbContext</param>
        /// <param name="efQuery">Entity Framework query</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Object data set with one populated table</returns>
        public static ObjectDataSet LoadData<TEntity>(DbContext dbContext, IQueryable<TEntity> efQuery, string tableName) where TEntity : class
        {
            ObjectDataSet objectDataSet = new ObjectDataSet();
            LoadData(dbContext, efQuery, objectDataSet, tableName);
            return objectDataSet;
        }

        /// <summary>
        /// Loads data into an existing object data set.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">DbContext</param>
        /// <param name="efQuery">Entity Framework query</param>
        /// <param name="targetObjectDataSet">Target object data set</param>
        /// <param name="tableName">Table name</param>
        public static void LoadData<TEntity>(DbContext dbContext, IQueryable<TEntity> efQuery, ObjectDataSet targetObjectDataSet, string tableName) where TEntity : class
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (efQuery == null)
            {
                throw new ArgumentNullException(nameof(efQuery));
            }

            if (targetObjectDataSet == null)
            {
                throw new ArgumentNullException(nameof(targetObjectDataSet));
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            ObjectTable<TEntity> objectTable = targetObjectDataSet.AddTable<TEntity>(tableName);
            objectTable.Items.AddRange(efQuery.ToList());
        }

        /// <summary>
        /// Loads data into a new object data set asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">DbContext</param>
        /// <param name="efQuery">Entity Framework query</param>
        /// <param name="tableName">Table name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object data set with one populated table</returns>
        public static async Task<ObjectDataSet> LoadDataAsync<TEntity>(DbContext dbContext, IQueryable<TEntity> efQuery, string tableName, CancellationToken cancellationToken = default) where TEntity : class
        {
            ObjectDataSet objectDataSet = new ObjectDataSet();
            await LoadDataAsync(dbContext, efQuery, objectDataSet, tableName, cancellationToken);
            return objectDataSet;
        }

        /// <summary>
        /// Loads data into an existing object data set asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">DbContext</param>
        /// <param name="efQuery">Entity Framework query</param>
        /// <param name="targetObjectDataSet">Target object data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task LoadDataAsync<TEntity>(DbContext dbContext, IQueryable<TEntity> efQuery, ObjectDataSet targetObjectDataSet, string tableName, CancellationToken cancellationToken = default) where TEntity : class
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (efQuery == null)
            {
                throw new ArgumentNullException(nameof(efQuery));
            }

            if (targetObjectDataSet == null)
            {
                throw new ArgumentNullException(nameof(targetObjectDataSet));
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            ObjectTable<TEntity> objectTable = targetObjectDataSet.AddTable<TEntity>(tableName);
            objectTable.Items.AddRange(await efQuery.ToListAsync(cancellationToken));
        }
        #endregion
    }
}
