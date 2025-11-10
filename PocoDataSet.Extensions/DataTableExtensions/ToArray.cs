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
        /// <param name="table">The data table to convert.</param>
        /// <returns>An array of <typeparamref name="TInterface"/> proxies backed by <see cref="IDataRow"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
        public static TInterface[] ToArray<TInterface>(this IDataTable table) where TInterface : class
        {
            List<TInterface> list = ToList<TInterface>(table);
            return list.ToArray();
        }

        /// <summary>
        /// Converts all rows in the specified <see cref="IDataTable"/> to an array using a custom selector.
        /// </summary>
        /// <typeparam name="T">The element type of the resulting array.</typeparam>
        /// <param name="table">The data table to convert.</param>
        /// <param name="selector">A delegate that creates a <typeparamref name="T"/> from an <see cref="IDataRow"/>.</param>
        /// <returns>An array of elements created from each row.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="table"/> or <paramref name="selector"/> is <c>null</c>.
        /// </exception>
        public static T[] ToArray<T>(this IDataTable table, Func<IDataRow, T> selector)
        {
            List<T> list = ToList(table, selector);
            return list.ToArray();
        }
        #endregion
    }
}
