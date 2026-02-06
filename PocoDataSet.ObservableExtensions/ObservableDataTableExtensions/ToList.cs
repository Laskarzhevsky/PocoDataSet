using System;
using System.Collections.Generic;
using System.Reflection;

using PocoDataSet.IData;
using PocoDataSet.Extensions;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

namespace PocoDataSet.ObservableExtensions
{
    public static partial class ObservableDataTableExtensions
    {
        /// <summary>
        /// Gets list of observable data rows
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <returns>List of observable data rows</returns>
        public static List<IObservableDataRow> ToList(this IObservableDataTable observableDataTable)
        {
            return new List<IObservableDataRow>(observableDataTable.Rows);
        }

        /// <summary>
        /// Projects each row of the observable table into a DispatchProxy that implements TInterface,
        /// using the provided nameMap when creating the proxy handler (if your handler supports it).
        /// </summary>
        public static List<TInterface> ToList<TInterface>(this IObservableDataTable table, IDictionary<string, string>? nameMap) where TInterface : class
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            List<TInterface> list = new List<TInterface>();
            foreach (ObservableDataRow observableDataRow in table.Rows)
            {
                IDataRow dataRow = observableDataRow.InnerDataRow;
                if (dataRow != null)
                {
                    TInterface proxy = DispatchProxy.Create<TInterface, InterfaceRowProxy<TInterface>>();
                    InterfaceRowProxy<TInterface> handler = (InterfaceRowProxy<TInterface>)(object)proxy;

                    if (nameMap == null)
                    {
                        handler.Initialize(dataRow, null);
                    }
                    else
                    {
                        handler.Initialize(dataRow, nameMap);
                    }

                    list.Add(proxy);
                }
            }

            return list;
        }
    }
}
