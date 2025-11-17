using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Converts all rows in the specified <see cref="IDataTable"/> to an array of interface proxies.
        /// </summary>
        /// <typeparam name="TInterface">The interface type describing the table's row contract.</typeparam>
        /// <param name="dataTable">The data table to convert.</param>
        /// <returns>An array of <typeparamref name="TInterface"/> proxies backed by <see cref="IDataRow"/>.</returns>
        public static TInterface[] ToArray<TInterface>(this IDataTable? dataTable) where TInterface : class
        {
            if (dataTable == null)
            {
                return Array.Empty<TInterface>();
            }

            List<TInterface> list = ToList<TInterface>(dataTable);
            return list.ToArray();
        }

        /// <summary>
        /// Converts all rows in the specified <see cref="IDataTable"/> to an array using a custom selector.
        /// </summary>
        /// <typeparam name="T">The element type of the resulting array.</typeparam>
        /// <param name="dataTable">The data table to convert.</param>
        /// <param name="selector">A delegate that creates a <typeparamref name="T"/> from an <see cref="IDataRow"/>.</param>
        /// <returns>An array of elements created from each row.</returns>
        public static T[] ToArray<T>(this IDataTable? dataTable, Func<IDataRow, T> selector)
        {
            if (dataTable == null)
            {
                return Array.Empty<T>();
            }

            List<T> list = ToList(dataTable, selector);
            return list.ToArray();
        }
        #endregion
    }
}
