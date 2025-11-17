using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data row extension methods
    /// </summary>
    public static partial class DataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Gets "live" data row as an interface
        /// </summary>
        /// <typeparam name="TInterface">POCO interface type</typeparam>
        /// <param name="dataRow">Data row</param>
        /// <returns>"Live" data row as an interface</returns>
        public static TInterface AsInterface<TInterface>(this IDataRow? dataRow) where TInterface : class
        {
            if (dataRow == null)
            {
                return default!;
            }

            return InterfaceRowProxy<TInterface>.CreateProxy(dataRow);
        }

        /// <summary>
        /// Gets "live" data row as an interface
        /// </summary>
        /// <typeparam name="TInterface">POCO interface type</typeparam>
        /// <param name="dataRow">Data row</param>
        /// <param name="nameMap">Name map</param>
        /// <returns>"Live" data row as an interface</returns>
        public static TInterface AsInterface<TInterface>(this IDataRow? dataRow, IDictionary<string, string> nameMap) where TInterface : class
        {
            if (dataRow == null)
            {
                return default!;
            }

            return InterfaceRowProxy<TInterface>.CreateProxy(dataRow, nameMap);
        }
        #endregion
    }
}
