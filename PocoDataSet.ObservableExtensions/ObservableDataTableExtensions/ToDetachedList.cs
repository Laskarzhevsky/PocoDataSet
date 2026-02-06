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
        /// Returns observable data table as a detached list
        /// </summary>
        /// <typeparam name="TInterface">Data table interface type</typeparam>
        /// <param name="observableDataTable">Observable data table</param>
        /// <returns>Observable data as a detached list</returns>
        public static List<TInterface> ToDetachedList<TInterface>(this IObservableDataTable? observableDataTable) where TInterface : class
        {
            if (observableDataTable == null)
            {
                return new List<TInterface>();
            }

            return observableDataTable.InnerDataTable.ToDetachedList<TInterface>();
        }

        /// <summary>
        /// Returns observable data table as a detached list
        /// </summary>
        /// <typeparam name="TInterface">Data table interface type</typeparam>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="nameMap">Name map</param>
        /// <returns>Observable data table as a detached list</returns>
        public static List<TInterface> ToDetachedList<TInterface>(this IObservableDataTable? observableDataTable, IDictionary<string, string> nameMap) where TInterface : class
        {
            if (observableDataTable == null)
            {
                return new List<TInterface>();
            }

            return observableDataTable.InnerDataTable.ToDetachedList<TInterface>(nameMap);
        }
        #endregion
    }
}
