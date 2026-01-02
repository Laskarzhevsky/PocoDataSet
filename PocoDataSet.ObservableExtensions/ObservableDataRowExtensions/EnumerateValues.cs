using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data row extension methods
    /// </summary>
    public static partial class ObservableDataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Enumerates values of the inner data row
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        /// <returns>Enumerated values as (ColumnName, Value) pairs</returns>
        public static IEnumerable<KeyValuePair<string, object?>> EnumerateValues(this IObservableDataRow? observableDataRow)
        {
            if (observableDataRow == null)
            {
                return new List<KeyValuePair<string, object?>>();
            }

            return observableDataRow.InnerDataRow.EnumerateValues();
        }
        #endregion
    }
}
