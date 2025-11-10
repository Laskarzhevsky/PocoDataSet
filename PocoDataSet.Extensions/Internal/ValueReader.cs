using System;
using System.Reflection;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal static class ValueReader
    {
        public static bool TryRead(IDataRow row, string column, Type targetType, out object? value)
        {
            value = null;
            try
            {
                // Nullable<T> handling
                Type? underlying = Nullable.GetUnderlyingType(targetType);
                Type effective = underlying ?? targetType;

                // Try strong GetXXX method (GetInt32, GetString, etc.)
                if (TryCall(row, effective, column, out value))
                {
                    return true;
                }

                // Try GetValue / indexer
                if (TryGetObject(row, column, out value))
                {
                    if (value == null || targetType.IsInstanceOfType(value))
                    {
                        return true;
                    }

                    try
                    {
                        value = ConvertTo(value, effective);
                        return true;
                    }
                    catch
                    {
                        // fallthrough to default
                    }
                }

                // If row has HasColumn/ContainsColumn, honor it (optional)
                var hasCol = row.GetType().GetMethod("HasColumn", new[] { typeof(string) })
                            ?? row.GetType().GetMethod("ContainsColumn", new[] { typeof(string) });
                if (hasCol != null)
                {
                    var ok = (bool)hasCol.Invoke(row, new object[] { column })!;
                    if (!ok)
                        return false;
                }

                return false;
            }
            catch
            {
                // Any binding/conversion/column exceptions → treat as missing
                value = null;
                return false;
            }
        }

        static bool TryCall(IDataRow row, Type effectiveType, string column, out object? value)
        {
            value = null;
            string? methodName = GetMethodName(effectiveType);
            if (methodName == null)
                return false;

            MethodInfo? mi = row.GetType().GetMethod(methodName, new[] { typeof(string) });
            if (mi == null)
                return false;

            try
            {
                value = mi.Invoke(row, new object[] { column });
                return true;
            }
            catch
            {
                return false;
            }
        }

        static string? GetMethodName(Type t)
        {
            if (t == typeof(string))
                return "GetString";
            if (t == typeof(int))
                return "GetInt32";
            if (t == typeof(long))
                return "GetInt64";
            if (t == typeof(short))
                return "GetInt16";
            if (t == typeof(byte))
                return "GetByte";
            if (t == typeof(bool))
                return "GetBoolean";
            if (t == typeof(decimal))
                return "GetDecimal";
            if (t == typeof(double))
                return "GetDouble";
            if (t == typeof(float))
                return "GetSingle";
            if (t == typeof(DateTime))
                return "GetDateTime";
            if (t == typeof(Guid))
                return "GetGuid";
            return null;
        }

        static bool TryGetObject(IDataRow row, string column, out object? value)
        {
            value = null;

            // GetValue(string)
            var mi = row.GetType().GetMethod("GetValue", new[] { typeof(string) });
            if (mi != null)
            {
                try
                {
                    value = mi.Invoke(row, new object[] { column });
                    return true;
                }
                catch { /* missing column or other issue */ }
            }

            // this[string]
            var indexer = row.GetType().GetProperty("Item", new[] { typeof(string) });
            if (indexer != null && indexer.GetIndexParameters().Length == 1)
            {
                try
                {
                    value = indexer.GetValue(row, new object[] { column });
                    return true;
                }
                catch { }
            }

            return false;
        }

        static object? ConvertTo(object value, Type target)
        {
            if (target.IsEnum)
            {
                if (value is string s)
                    return Enum.Parse(target, s, true);
                return Enum.ToObject(target, Convert.ChangeType(value, Enum.GetUnderlyingType(target)));
            }
            return Convert.ChangeType(value, target);
        }
    }
}
