using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Developer-experience facade that mirrors POCO DataSet merge semantics at the EF Core boundary.
    /// </summary>
    public static class EfCoreMergeAdapter
    {
        #region Public Methods
        /// <summary>
        /// Loads data into data set for an initial UI baseline or full refresh, returning a snapshot dataset that can be merged into the UI baseline.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">DB context</param>
        /// <param name="efQuery">Entity Framework query</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Data set with loaded data</returns>
        public static IDataSet LoadData<TEntity>(DbContext dbContext, IQueryable<TEntity> efQuery, string tableName) where TEntity : class
        {
            IDataSet dataSet = new DataSet();
            efQuery.ToDataTable(dbContext, dataSet, tableName);
            dataSet.ApplyEfModelKeysAndRelations(dbContext);
            return dataSet;
        }

        /// <summary>
        /// Loads data into data set for an initial UI baseline or full refresh, returning a snapshot dataset that can be merged into the UI baseline.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">DB context</param>
        /// <param name="efQuery">Entity Framework query</param>
        /// <param name="targetDataSet">Target data set</param>
        /// <param name="tableName">Table name</param>
        /// <exception cref="ArgumentNullException">Thrown if table name is not provided</exception>
        public static void LoadData<TEntity>(DbContext dbContext, IQueryable<TEntity> efQuery, IDataSet targetDataSet, string tableName) where TEntity : class
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            efQuery.ToDataTable(dbContext, targetDataSet, tableName);
        }

        /// <summary>
        /// Saves data and returns post-save data set with server-confirmed values (e.g. identity column values, concurrency tokens, etc.) after save.
        /// </summary>
        /// <param name="dbContext">DB context</param>
        /// <param name="changeset">Change set</param>
        /// <returns>Result of saving - the post-save data set</returns>
        public static IDataSet SaveData(DbContext dbContext, IDataSet changeset)
        {
            return EfChangesetDataSetApplier.ApplyChangesetAndSaveReturningPostSaveResponse(dbContext, changeset);
        }
        #endregion
    }
}
