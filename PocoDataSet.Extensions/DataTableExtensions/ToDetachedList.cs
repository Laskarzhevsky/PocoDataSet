using System;
using System.Collections.Generic;
using System.Reflection;

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
        /// Returns data table as a detached list
        /// </summary>
        /// <typeparam name="TInterface">data table interface type</typeparam>
        /// <param name="dataTable">Data table</param>
        /// <returns>Data table as a detached list</returns>
        public static List<TInterface> ToDetachedList<TInterface>(this IDataTable? dataTable) where TInterface : class
        {
            var list = new List<TInterface>();
            if (dataTable == null)
            {
                return list;
            }

            foreach (IDataRow row in dataTable.Rows)
            {
                if (row == null)
                {
                    continue;
                }

                TInterface proxy = DispatchProxy.Create<TInterface, SnapshotInterfaceProxy>();
                ((SnapshotInterfaceProxy)(object)proxy).Initialize(typeof(TInterface), row);
                list.Add(proxy);
            }

            return list;
        }

        /// <summary>
        /// Returns data table as a detached list
        /// </summary>
        /// <typeparam name="TInterface">data table interface type</typeparam>
        /// <param name="dataTable">Data table</param>
        /// <param name="nameMap">Name map</param>
        /// <returns>Data table as a detached list</returns>
        public static List<TInterface> ToDetachedList<TInterface>(this IDataTable? table, IDictionary<string, string> nameMap) where TInterface : class
        {
            List<TInterface> list = new List<TInterface>();
            if (table == null)
            {
                return list;
            }

            if (nameMap == null)
            {
                throw new ArgumentNullException(nameof(nameMap));
            }

            foreach (IDataRow row in table.Rows)
            {
                if (row == null)
                {
                    continue;
                }

                TInterface proxy = DispatchProxy.Create<TInterface, SnapshotInterfaceProxy>();
                ((SnapshotInterfaceProxy)(object)proxy).Initialize(typeof(TInterface), row, nameMap);
                list.Add(proxy);
            }

            return list;
        }
        #endregion
    }
}
