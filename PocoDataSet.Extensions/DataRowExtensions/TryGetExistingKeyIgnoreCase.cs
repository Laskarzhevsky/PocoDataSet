using System;
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
        private static bool TryGetValueIgnoreCase(IDataRow dataRow, string requestedKey, out object? value)
        {
            // Fast path
            if (dataRow.TryGetValue(requestedKey, out value))
            {
                return true;
            }

            string existingKey;
            if (TryGetExistingKeyIgnoreCase(dataRow, requestedKey, out existingKey))
            {
                return dataRow.TryGetValue(existingKey, out value);
            }

            value = null;
            return false;
        }
        #endregion
    }
}
