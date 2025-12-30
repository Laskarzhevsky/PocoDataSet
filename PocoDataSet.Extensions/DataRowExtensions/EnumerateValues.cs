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
        /// Enumerates values
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>Enumerated values</returns>
        public static IEnumerable<KeyValuePair<string, object?>> EnumerateValues(this IDataRow dataRow)
        {
            foreach (KeyValuePair<string, object?> pair in dataRow.Values)
            {
                yield return pair;
            }
        }
        #endregion
    }
}
