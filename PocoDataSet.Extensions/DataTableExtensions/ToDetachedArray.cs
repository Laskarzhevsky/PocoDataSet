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
        /// Returns data table as a detached array
        /// </summary>
        /// <typeparam name="TInterface">Data table interface type</typeparam>
        /// <param name="dataTable">Data table</param>
        /// <returns>Data table as a detached array</returns>
        public static TInterface[] ToDetachedArray<TInterface>(this IDataTable dataTable) where TInterface : class
        {
            List<TInterface> list = ToDetachedList<TInterface>(dataTable);
            return list.ToArray();
        }

        /// <summary>
        /// Returns data table as a detached array
        /// </summary>
        /// <typeparam name="TInterface">Data table interface type</typeparam>
        /// <param name="dataTable">Data table</param>
        /// <param name="nameMap">Name map</param>
        /// <returns>Data table as a detached array</returns>
        public static TInterface[] ToDetachedArray<TInterface>(this IDataTable dataTable, IDictionary<string, string> nameMap) where TInterface : class
        {
            List<TInterface> list = ToDetachedList<TInterface>(dataTable, nameMap);
            return list.ToArray();
        }
        #endregion
    }
}
