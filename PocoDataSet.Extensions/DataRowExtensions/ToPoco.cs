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
        /// Converts data row into POCO
        /// </summary>
        /// <typeparam name="T">POCO type</typeparam>
        /// <param name="dataRow">Data row</param>
        /// <returns>POCO</returns>
        public static T ToPoco<T>(this IDataRow? dataRow) where T : new()
        {
            T stronglyTypedObject = new T();
            if (dataRow == null)
            {
                return stronglyTypedObject;
            }

            Type targetType = typeof(T);
            PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                string propertyName = property.Name;
                if (!dataRow.TryGetValue(propertyName, out object? value) || value == null)
                {
                    continue;
                }

                try
                {
                    Type propertyType = property.PropertyType;
                    if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                    {
                        if (value is Guid guidValue)
                        {
                            property.SetValue(stronglyTypedObject, guidValue);
                        }
                        else if (value is string guidStr && Guid.TryParse(guidStr, out Guid parsedGuid))
                        {
                            property.SetValue(stronglyTypedObject, parsedGuid);
                        }

                        continue;
                    }

                    if (propertyType.IsEnum)
                    {
                        object? enumValue = null;
                        if (value is string enumString)
                        {
                            enumValue = Enum.Parse(propertyType, enumString, ignoreCase: true);
                        }
                        else
                        {
                            enumValue = Enum.ToObject(propertyType, value);
                        }

                        property.SetValue(stronglyTypedObject, enumValue);
                        continue;
                    }

                    Type? underlying = Nullable.GetUnderlyingType(propertyType);
                    object safeValue = Convert.ChangeType(value, underlying ?? propertyType);
                    property.SetValue(stronglyTypedObject, safeValue);
                }
                catch
                {
                    // Optionally log or continue
                }
            }

            return stronglyTypedObject;
        }
        #endregion
    }
}
