using System;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data row extension methods
    /// </summary>
    public static partial class DataRowExtensions
    {
        #region Public Methods
        private static bool TryGetExistingKeyIgnoreCase(IDataRow dataRow, string requestedKey, out string existingKey)
        {
            // Fast path (exact match)
            if (dataRow.ContainsKey(requestedKey))
            {
                existingKey = requestedKey;
                return true;
            }

            // Scan existing keys ignoring case
            foreach (string key in dataRow.Values.Keys)
            {
                if (string.Equals(key, requestedKey, StringComparison.OrdinalIgnoreCase))
                {
                    existingKey = key;
                    return true;
                }
            }

            existingKey = requestedKey;
            return false;
        }
        #endregion
    }
}
