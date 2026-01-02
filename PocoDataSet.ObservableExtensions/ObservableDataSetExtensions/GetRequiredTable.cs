using System.Collections.Generic;

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
        /// Gets observable table by name or throws
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Observable table</returns>
        /// <exception cref="KeyNotFoundException">Thrown if table does not exist</exception>
        public static IObservableDataTable GetRequiredTable(this IObservableDataSet? observableDataSet, string tableName)
        {
            IObservableDataTable? table = observableDataSet.GetTable(tableName);
            if (table == null)
            {
                throw new KeyNotFoundException($"ObservableDataSet does not contain table with name {tableName}.");
            }

            return table;
        }
        #endregion
    }
}
