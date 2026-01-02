using System.Collections.Generic;

using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Gets observable table by name
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Observable table or null</returns>
        public static IObservableDataTable? GetTable(this IObservableDataSet? observableDataSet, string tableName)
        {
            if (observableDataSet == null)
            {
                return null;
            }

            IDictionary<string, IObservableDataTable> tables = observableDataSet.Tables;
            IObservableDataTable? table;
            if (tables.TryGetValue(tableName, out table))
            {
                return table;
            }

            return null;
        }
        #endregion
    }
}
