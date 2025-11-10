using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        public static bool TryGetTable(this IDataSet dataSet, string? tableName, out IDataTable? table)
        {
            table = null;

            if (dataSet == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                return false;
            }

            // Treat Tables as an IDictionary to support TryGetValue and enumeration.
            IDictionary<string, IDataTable>? dict = dataSet.Tables as IDictionary<string, IDataTable>;
            if (dict == null)
            {
                return false;
            }

            // 1) Fast path: exact key
            IDataTable? found;
            if (dict.TryGetValue(tableName, out found))
            {
                table = found;
                return true;
            }

            // 2) Fallback: case-insensitive scan (helps if callers vary casing)
            foreach (KeyValuePair<string, IDataTable> kv in dict)
            {
                if (string.Equals(kv.Key, tableName, StringComparison.OrdinalIgnoreCase))
                {
                    table = kv.Value;
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
