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
    public static class EfCoreBridgeExtensions
    {

/// <summary>
/// Projects an EF Core query into a PocoDataSet table, building column metadata from the EF Core model.
/// </summary>
/// <typeparam name="T">POCO/entity type.</typeparam>
/// <param name="query">EF Core query.</param>
/// <param name="dbContext">DbContext used to obtain EF model metadata.</param>
/// <param name="dataSet">Target data set.</param>
/// <param name="tableName">Target table name.</param>
/// <returns>Created and populated table.</returns>
public static IDataTable ToDataTable<T>(
    this IQueryable<T> query,
    DbContext dbContext,
    IDataSet dataSet,
    string tableName)
    where T : class
{
    if (dbContext == null)
    {
        throw new System.ArgumentNullException(nameof(dbContext));
    }

    List<IColumnMetadata> schema = EfColumnMetadataBuilder.Build<T>(dbContext);

    return query.ToDataTable(dataSet, tableName, schema);
}

/// <summary>
/// Projects an EF Core query into a PocoDataSet table, building column metadata from the EF Core model.
/// </summary>
/// <typeparam name="T">POCO/entity type.</typeparam>
/// <param name="query">EF Core query.</param>
/// <param name="dbContext">DbContext used to obtain EF model metadata.</param>
/// <param name="dataSet">Target data set.</param>
/// <param name="tableName">Target table name.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Created and populated table.</returns>
public static async Task<IDataTable> ToDataTableAsync<T>(
    this IQueryable<T> query,
    DbContext dbContext,
    IDataSet dataSet,
    string tableName,
    CancellationToken cancellationToken = default)
    where T : class
{
    if (dbContext == null)
    {
        throw new System.ArgumentNullException(nameof(dbContext));
    }

    List<IColumnMetadata> schema = EfColumnMetadataBuilder.Build<T>(dbContext);

    return await query.ToDataTableAsync(dataSet, tableName, schema, cancellationToken).ConfigureAwait(false);
}


        /// <summary>
        /// Materializes the query (ToList) and creates a data table in the target data set, then populates
        /// table rows from the returned POCO list using PocoDataSet.Extensions mapping helpers.
        /// </summary>
        /// <typeparam name="T">POCO/entity type.</typeparam>
        /// <param name="query">EF Core query.</param>
        /// <param name="dataSet">Target data set.</param>
        /// <param name="tableName">Target table name.</param>
        /// <param name="listOfColumnMetadata">Column schema to create on the table.</param>
        /// <returns>Created and populated table.</returns>
        public static IDataTable ToDataTable<T>(this IQueryable<T> query, IDataSet dataSet, string tableName, List<IColumnMetadata> listOfColumnMetadata)
        {
            List<T> items = query.ToList();
            return dataSet.PocoListToDataTable(tableName, items, listOfColumnMetadata);
        }

        /// <summary>
        /// Materializes the query asynchronously (ToListAsync) and creates a data table in the target data set, then populates
        /// table rows from the returned POCO list using PocoDataSet.Extensions mapping helpers.
        /// </summary>
        /// <typeparam name="T">POCO/entity type.</typeparam>
        /// <param name="query">EF Core query.</param>
        /// <param name="dataSet">Target data set.</param>
        /// <param name="tableName">Target table name.</param>
        /// <param name="listOfColumnMetadata">Column schema to create on the table.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created and populated table.</returns>
        public static async Task<IDataTable> ToDataTableAsync<T>(this IQueryable<T> query, IDataSet dataSet, string tableName, List<IColumnMetadata> listOfColumnMetadata, CancellationToken cancellationToken = default)
        {
            List<T> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return dataSet.PocoListToDataTable(tableName, items, listOfColumnMetadata);
        }
    }
}
