using System;
using System.Reflection;

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
        /// Copies public readable properties from a POCO into a data row.
        /// Matching between property names and existing row keys is case-insensitive.
        /// If a matching key exists (ignoring case), the existing key is updated to avoid duplicates.
        /// If no matching key exists, a new key is added using the property name.
        /// </summary>
        /// <typeparam name="T">POCO type.</typeparam>
        /// <param name="dataRow">Target data row.</param>
        /// <param name="poco">Source POCO instance.</param>
        public static void CopyFromPoco<T>(this IDataRow? dataRow, T? poco)
        {
            if (dataRow == null)
            {
                return;
            }

            if (poco == null)
            {
                return;
            }

            Type sourceType = typeof(T);
            PropertyInfo[] properties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                if (!property.CanRead)
                {
                    continue;
                }

                string propertyName = property.Name;

                object? propertyValue = property.GetValue(poco);

                // Prefer an existing key ignoring case to avoid duplicates (e.g., "name" vs "Name")
                string existingKey;
                if (TryGetExistingKeyIgnoreCase(dataRow, propertyName, out existingKey))
                {
                    dataRow[existingKey] = propertyValue;
                }
                else
                {
                    dataRow[propertyName] = propertyValue;
                }
            }
        }
        #endregion
    }
}
