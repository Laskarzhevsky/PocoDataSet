using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Adds a new observable table to observable data set using POCO interface type (delegates to inner data set)
        /// </summary>
        /// <typeparam name="TInterface">Interface type</typeparam>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="interfaceType">Interface type</param>
        /// <returns>New observable table</returns>
        /// <exception cref="KeyDuplicationException">Exception is thrown if dataset contains a table with specified name already</exception>
        public static IObservableDataTable AddNewTableFromPocoInterface(this IObservableDataSet? observableDataSet, string tableName, Type interfaceType)
        {
            if (observableDataSet == null)
            {
                return default!;
            }

            IDataTable dataTable = observableDataSet.InnerDataSet.AddNewTableFromPocoInterface(tableName, interfaceType);
            IObservableDataTable observableDataTable = observableDataSet.AddObservableTable(dataTable);
            return observableDataTable;
        }
        #endregion
    }
}
