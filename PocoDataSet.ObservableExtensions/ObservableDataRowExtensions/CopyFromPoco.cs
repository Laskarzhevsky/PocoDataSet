using System;
using System.Reflection;

using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data row extension methods.
    /// </summary>
    public static partial class ObservableDataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Copies public readable POCO properties into an observable data row.
        /// Matching between property names and existing row keys is case-insensitive.
        /// </summary>
        /// <typeparam name="T">POCO type</typeparam>
        /// <param name="observableDataRow">Target observable data row</param>
        /// <param name="pocoInstance">Source POCO instance</param>
        public static void CopyFromPoco<T>(this IObservableDataRow? observableDataRow, T? pocoInstance)
        {
            if (observableDataRow == null)
            {
                return;
            }

            if (pocoInstance == null)
            {
                return;
            }

            Type pocoType = pocoInstance.GetType();
            PropertyInfo[] properties = pocoType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (!property.CanRead)
                {
                    continue;
                }

                string propertyName = property.Name;
                object? propertyValue = property.GetValue(pocoInstance, null);
                string existingKey;
                if (observableDataRow.TryGetFieldKeyByColumnName(propertyName, out existingKey))
                {
                    observableDataRow[existingKey] = propertyValue;
                }
                else
                {
                    // If the row implementation allows adding new keys, this will work.
                    // If not, it will throw, matching the non-observable behavior.
                    observableDataRow[propertyName] = propertyValue;
                }
            }
        }
        #endregion
    }
}
