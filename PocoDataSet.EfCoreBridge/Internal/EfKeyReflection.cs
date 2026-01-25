using System;
using System.Reflection;

namespace PocoDataSet.EfCoreBridge
{
    internal static class EfKeyReflection
    {
        #region Public Methods
        /// <summary>
        /// Gets required property ignoring case
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <param name="propertyName">Property name</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static PropertyInfo GetRequiredPropertyIgnoreCase(Type entityType, string propertyName)
        {
            PropertyInfo? propertyInfo = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propertyInfo == null)
            {
                throw new InvalidOperationException(
                    "EF bridge: entity type '" + entityType.FullName +
                    "' does not contain public property '" + propertyName + "'.");
            }

            return propertyInfo;
        }

        /// <summary>
        /// Tries to set key
        /// </summary>
        /// <param name="keyProperty">Key property</param>
        /// <param name="entity">Entity to set key on</param>
        /// <param name="keyValue">Key value</param>
        internal static void TrySetKey(PropertyInfo keyProperty, object entity, object? keyValue)
        {
            if (keyValue == null)
            {
                return;
            }

            Type targetType = keyProperty.PropertyType;
            Type? underlying = Nullable.GetUnderlyingType(targetType);
            Type effective = underlying ?? targetType;

            try
            {
                if (effective.IsEnum)
                {
                    if (keyValue is string s)
                    {
                        object enumValue = Enum.Parse(effective, s, true);
                        keyProperty.SetValue(entity, enumValue);
                        return;
                    }

                    object enumValue2 = Enum.ToObject(effective, keyValue);
                    keyProperty.SetValue(entity, enumValue2);
                    return;
                }

                if (effective == typeof(Guid))
                {
                    if (keyValue is Guid g)
                    {
                        keyProperty.SetValue(entity, g);
                        return;
                    }

                    if (keyValue is string gs)
                    {
                        Guid parsed;
                        if (Guid.TryParse(gs, out parsed))
                        {
                            keyProperty.SetValue(entity, parsed);
                        }

                        return;
                    }
                }

                object converted = Convert.ChangeType(keyValue, effective);
                keyProperty.SetValue(entity, converted);
            }
            catch
            {
                // swallow
            }
        }
        #endregion
    }
}
