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
        /// Copies values from an observable data row into a POCO instance by assigning writable properties.
        /// Matching between property names and existing row keys is case-insensitive.
        /// </summary>
        /// <typeparam name="T">POCO type</typeparam>
        /// <param name="observableDataRow">Source observable data row</param>
        /// <param name="pocoInstance">Target POCO instance to populate</param>
        public static void CopyToPoco<T>(this IObservableDataRow? observableDataRow, T? pocoInstance)
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

                if (!property.CanWrite)
                {
                    continue;
                }

                string propertyName = property.Name;

                object? value;
                if (observableDataRow.TryGetValue(propertyName, out value))
                {
                    if (value == DBNull.Value)
                    {
                        value = null;
                    }

                    AssignValue(pocoInstance, property, value);
                }
            }
        }
        #endregion

        #region Private Methods
        private static void AssignValue<T>(T pocoInstance, PropertyInfo property, object? value)
        {
            Type propertyType = property.PropertyType;

            if (value == null)
            {
                // For nullable types and reference types, null assignment is OK.
                if (!propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) != null)
                {
                    property.SetValue(pocoInstance, null, null);
                }

                return;
            }

            Type targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            object? convertedValue = value;

            if (targetType.IsEnum)
            {
                if (value is string s)
                {
                    convertedValue = Enum.Parse(targetType, s, ignoreCase: true);
                }
                else
                {
                    convertedValue = Enum.ToObject(targetType, value);
                }
            }
            else if (targetType == typeof(Guid))
            {
                if (value is string guidString)
                {
                    convertedValue = Guid.Parse(guidString);
                }
            }
            else if (targetType == typeof(DateTime))
            {
                if (value is string dtString)
                {
                    convertedValue = DateTime.Parse(dtString, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
                }
            }
            else
            {
                if (value != null && !targetType.IsAssignableFrom(value.GetType()))
                {
                    try
                    {
                        convertedValue = Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        convertedValue = value;
                    }
                }
            }

            property.SetValue(pocoInstance, convertedValue, null);
        }
        #endregion
    }
}
