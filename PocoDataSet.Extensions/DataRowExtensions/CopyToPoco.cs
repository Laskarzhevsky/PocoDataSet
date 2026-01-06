using System;
using System.Globalization;
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
        /// Copies values from a data row into an existing POCO instance.
        /// Matching between property names and row keys is case-insensitive.
        /// </summary>
        /// <typeparam name="T">POCO type.</typeparam>
        /// <param name="dataRow">Source data row.</param>
        /// <param name="pocoInstance">Target POCO instance.</param>
        public static void CopyToPoco<T>(this IDataRow? dataRow, T? pocoInstance)
        {
            if (dataRow == null)
            {
                return;
            }

            if (pocoInstance == null)
            {
                return;
            }

            Type targetType = typeof(T);
            PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                if (!property.CanWrite)
                {
                    continue;
                }

                string propertyName = property.Name;

                object? value;
                if (!TryGetValue(dataRow, propertyName, out value))
                {
                    continue;
                }

                if (value == null)
                {
                    try
                    {
                        property.SetValue(pocoInstance, null);
                    }
                    catch
                    {
                        // swallow
                    }

                    continue;
                }

                try
                {
                    Type propertyType = property.PropertyType;

                    // Nullable<T> support
                    Type? nullableUnderlying = Nullable.GetUnderlyingType(propertyType);
                    Type effectiveType = nullableUnderlying ?? propertyType;

                    // Enum / Nullable<Enum>
                    if (effectiveType.IsEnum)
                    {
                        object enumValue;

                        if (value is string s)
                        {
                            enumValue = Enum.Parse(effectiveType, s, true);
                        }
                        else
                        {
                            enumValue = Enum.ToObject(effectiveType, value);
                        }

                        property.SetValue(pocoInstance, enumValue);
                        continue;
                    }

                    // Guid / Nullable<Guid>
                    if (effectiveType == typeof(Guid))
                    {
                        if (value is Guid guidValue)
                        {
                            property.SetValue(pocoInstance, guidValue);
                            continue;
                        }

                        if (value is string guidStr)
                        {
                            Guid parsedGuid;
                            if (Guid.TryParse(guidStr, out parsedGuid))
                            {
                                property.SetValue(pocoInstance, parsedGuid);
                            }

                            continue;
                        }

                        // fallthrough to Convert.ChangeType (will likely throw -> swallowed)
                    }

                    // DateTime / Nullable<DateTime>
                    if (effectiveType == typeof(DateTime))
                    {
                        if (value is DateTime dt)
                        {
                            property.SetValue(pocoInstance, dt);
                            continue;
                        }

                        if (value is string dtStr)
                        {
                            DateTime parsed;
                            if (DateTime.TryParse(
                                dtStr,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.RoundtripKind,
                                out parsed))
                            {
                                property.SetValue(pocoInstance, parsed);
                            }

                            continue;
                        }

                        // fallthrough to Convert.ChangeType (will likely throw -> swallowed)
                    }

                    object safeValue = Convert.ChangeType(value, effectiveType, CultureInfo.InvariantCulture);
                    property.SetValue(pocoInstance, safeValue);
                }
                catch
                {
                    // swallow (consistent with existing ToPoco/CopyToPoco behavior)
                }
            }
        }
        #endregion
    }
}
