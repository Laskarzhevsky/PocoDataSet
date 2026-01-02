using System;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data table extension methods
    /// </summary>
    public static partial class ObservableDataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Returns inner data table as a detached array.
        /// </summary>
        /// <typeparam name="TInterface">Data table interface type.</typeparam>
        /// <param name="observableDataTable">Observable data table.</param>
        /// <returns>Inner data table as a detached array.</returns>
        public static TInterface[] ToDetachedArray<TInterface>(this IObservableDataTable? observableDataTable) where TInterface : class
        {
            if (observableDataTable == null)
            {
                return Array.Empty<TInterface>();
            }

            return observableDataTable.InnerDataTable.ToDetachedArray<TInterface>();
        }

        /// <summary>
        /// Returns inner data table as a detached array.
        /// </summary>
        /// <typeparam name="TInterface">Data table interface type.</typeparam>
        /// <param name="observableDataTable">Observable data table.</param>
        /// <param name="nameMap">Name map.</param>
        /// <returns>Inner data table as a detached array.</returns>
        public static TInterface[] ToDetachedArray<TInterface>(this IObservableDataTable? observableDataTable, IDictionary<string, string> nameMap) where TInterface : class
        {
            if (observableDataTable == null)
            {
                return Array.Empty<TInterface>();
            }

            return observableDataTable.InnerDataTable.ToDetachedArray<TInterface>(nameMap);
        }
        #endregion
    }
}
