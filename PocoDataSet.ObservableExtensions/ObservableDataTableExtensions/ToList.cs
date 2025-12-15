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
        /// Returns a shallow copy of the table's observable rows.
        /// Keeping a copy avoids accidental mutation while iterating/sorting.
        /// </summary>
        public static List<IObservableDataRow> ToList(this IObservableDataTable table)
        {
            return new List<IObservableDataRow>(table.Rows);
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

            // Assumes IObservableDataTable.Rows is enumerable of IObservableDataRow and that
            // IObservableDataRow implements IDataRow (recommended). If so, the 'foreach (IDataRow ...)'
            // cast is valid. If your IObservableDataRow does NOT implement IDataRow, expose the inner
            // IDataRow (e.g., .Inner) and adjust below accordingly.
            foreach (ObservableDataRow observableDataRow in table.Rows)
            {
                IDataRow dataRow = observableDataRow.InnerDataRow;
                if (dataRow != null)
                {
                    TInterface proxy = DispatchProxy.Create<TInterface, InterfaceRowProxy<TInterface>>();
                    InterfaceRowProxy<TInterface> handler = (InterfaceRowProxy<TInterface>)(object)proxy;

                    if (nameMap == null)
                    {
                        // If your Initialize signature is Initialize(IDataRow, IDictionary<string,string>?)
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
