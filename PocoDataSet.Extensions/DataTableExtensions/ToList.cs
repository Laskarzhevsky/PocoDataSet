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
        /// Returns data table as a list of data rows
        /// </summary>
        /// <typeparam name="TInterface">Data table interface type</typeparam>
        /// <param name="dataTable">Data table</param>
        /// <returns>Data table as a list of data rows</returns>
        public static List<TInterface> ToList<TInterface>(this IDataTable? dataTable) where TInterface : class
        {
            if (dataTable == null)
            {
                return new List<TInterface>();
            }

            List<TInterface> list = new List<TInterface>(dataTable.Rows.Count);
            foreach (IDataRow dataRow in dataTable.Rows)
            {
                if (dataRow != null)
                {
                    // Reuse your extension that wraps InterfaceRowProxy<TInterface>.CreateProxy(...)
                    TInterface proxy = dataRow.AsInterface<TInterface>();
                    list.Add(proxy);
                }
            }

            return list;
        }

        /// <summary>
        /// Returns data table as a list of data rows
        /// </summary>
        /// <typeparam name="TInterface">Data table interface type</typeparam>
        /// <param name="dataTable">Data table</param>
        /// <param name="nameMap">Name map</param>
        /// <returns>Data table as a list of data rows</returns>
        public static List<TInterface> ToList<TInterface>(this IDataTable? table, IDictionary<string, string> nameMap) where TInterface : class
        {
            if (table == null)
            {
                return new List<TInterface>();
            }

            if (nameMap == null)
            {
                throw new ArgumentNullException(nameof(nameMap));
            }

            List<TInterface> list = new List<TInterface>(table.Rows.Count);

            foreach (IDataRow dataRow in table.Rows)
            {
                if (dataRow != null)
                {
                    TInterface proxy = dataRow.AsInterface<TInterface>(nameMap);
                    list.Add(proxy);
                }
            }

            return list;
        }

        /// <summary>
        /// Returns list of selected data rows
        /// </summary>
        /// <typeparam name="TInterface">Data table interface type</typeparam>
        /// <param name="dataTable">Data table</param>
        /// <param name="rowSelectionFunction"></param>
        /// <returns>List of selected data rows</returns>
        public static List<T> ToList<T>(this IDataTable? dataTable, Func<IDataRow, T> rowSelectionFunction)
        {
            if (dataTable == null)
            {
                return new List<T>();
            }

            if (rowSelectionFunction == null)
            {
                throw new ArgumentNullException(nameof(rowSelectionFunction));
            }

            List<T> listOfSelectedDataRows = new List<T>();
            foreach (IDataRow dataRow in dataTable.Rows)
            {
                if (dataRow != null)
                {
                    listOfSelectedDataRows.Add(rowSelectionFunction(dataRow));
                }
            }

            return listOfSelectedDataRows;
        }
        #endregion
    }
}
