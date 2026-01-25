using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using PocoDataSet.IData;
using PocoDataSet.Extensions;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// EF Core bridge helpers for projecting query results into PocoDataSet.
    /// </summary>
    public static partial class EfCoreBridgeExtensions
    {
        #region Public Methods
        /// <summary>
        /// Projects an EF Core query into a PocoDataSet table, building column metadata from the EF Core model.
        /// </summary>
        /// <typeparam name="T">POCO/entity type.</typeparam>
        /// <param name="efQuery">EF Core query.</param>
        /// <param name="dbContext">DbContext used to obtain EF model metadata.</param>
        /// <param name="dataSet">Target data set.</param>
        /// <param name="tableName">Target table name.</param>
        /// <returns>Created and populated table.</returns>
        public static IDataTable ToDataTable<T>(this IQueryable<T> efQuery, DbContext dbContext, IDataSet dataSet, string tableName) where T : class
        {
            List<IColumnMetadata> lisOfColumnMetadata = EfColumnMetadataBuilder.Build<T>(dbContext);

            return efQuery.ToDataTable(dataSet, tableName, lisOfColumnMetadata);
        }

        /// <summary>
        /// Projects an EF Core query into a PocoDataSet table, building column metadata from the EF Core model.
        /// </summary>
        /// <typeparam name="T">POCO/entity type.</typeparam>
        /// <param name="efQuery">EF Core query.</param>
        /// <param name="dbContext">DbContext used to obtain EF model metadata.</param>
        /// <param name="dataSet">Target data set.</param>
        /// <param name="tableName">Target table name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created and populated table.</returns>
        public static async Task<IDataTable> ToDataTableAsync<T>(this IQueryable<T> efQuery, DbContext dbContext, IDataSet dataSet, string tableName, CancellationToken cancellationToken = default) where T : class
        {
            List<IColumnMetadata> lisOfColumnMetadata = EfColumnMetadataBuilder.Build<T>(dbContext);

            return await efQuery.ToDataTableAsync(dataSet, tableName, lisOfColumnMetadata, cancellationToken).ConfigureAwait(false);
        }


        /// <summary>
        /// Materializes the query (ToList) and creates a data table in the target data set, then populates
        /// table rows from the returned POCO list using PocoDataSet.Extensions mapping helpers.
        /// </summary>
        /// <typeparam name="T">POCO/entity type.</typeparam>
        /// <param name="efQuery">EF Core query.</param>
        /// <param name="dataSet">Target data set.</param>
        /// <param name="tableName">Target table name.</param>
        /// <param name="listOfColumnMetadata">Column schema to create on the table.</param>
        /// <returns>Created and populated table.</returns>
        public static IDataTable ToDataTable<T>(this IQueryable<T> efQuery, IDataSet dataSet, string tableName, List<IColumnMetadata> listOfColumnMetadata)
        {
            List<T> items = efQuery.ToList();
            return dataSet.PocoListToDataTable(tableName, items, listOfColumnMetadata);
        }

        /// <summary>
        /// Materializes the query asynchronously (ToListAsync) and creates a data table in the target data set, then populates
        /// table rows from the returned POCO list using PocoDataSet.Extensions mapping helpers.
        /// </summary>
        /// <typeparam name="T">POCO/entity type.</typeparam>
        /// <param name="efQuery">EF Core query.</param>
        /// <param name="dataSet">Target data set.</param>
        /// <param name="tableName">Target table name.</param>
        /// <param name="listOfColumnMetadata">Column schema to create on the table.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created and populated table.</returns>
        public static async Task<IDataTable> ToDataTableAsync<T>(this IQueryable<T> efQuery, IDataSet dataSet, string tableName, List<IColumnMetadata> listOfColumnMetadata, CancellationToken cancellationToken = default)
        {
            List<T> items = await efQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            return dataSet.PocoListToDataTable(tableName, items, listOfColumnMetadata);
        }
        #endregion
    }
}
