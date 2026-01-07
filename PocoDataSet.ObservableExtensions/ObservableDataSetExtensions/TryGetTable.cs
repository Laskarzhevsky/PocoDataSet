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
        /// Tries to get observable table by name
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="observableDataTable">Observable table</param>
        /// <returns>True if found, otherwise false</returns>
        public static bool TryGetTable(this IObservableDataSet? observableDataSet, string tableName, out IObservableDataTable? observableDataTable)
        {
            observableDataTable = null;
            if (observableDataSet == null)
            {
                return false;
            }

            return observableDataSet.Tables.TryGetValue(tableName, out observableDataTable);
        }
        #endregion
    }
}
