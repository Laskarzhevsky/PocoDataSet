using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Serializer
{
    internal static class ValueNormalization
    {
        public static object? ConvertValueToColumnType(string dataType, object? value)
        {
            if (value == null)
            {
                return null;
            }

            // Already correct
            if (dataType == DataTypeNames.OBJECT || dataType == DataTypeNames.JSON)
            {
                return value;
            }

            // Normalize numerics coming from JSON (usually Int64)
            if (dataType == DataTypeNames.INT32)
            {
                if (value is long l)
                    return checked((int)l);
                if (value is int)
                    return value;
                if (value is decimal dec)
                    return checked((int)dec);
                if (value is double d)
                    return checked((int)d);
                if (value is string s && int.TryParse(s, out int i))
                    return i;
            }
            else if (dataType == DataTypeNames.INT64)
            {
                if (value is long)
                    return value;
                if (value is int i)
                    return (long)i;
                if (value is decimal dec)
                    return checked((long)dec);
                if (value is double d)
                    return checked((long)d);
                if (value is string s && long.TryParse(s, out long l2))
                    return l2;
            }
            else if (dataType == DataTypeNames.INT16)
            {
                if (value is long l)
                    return checked((short)l);
                if (value is int i)
                    return checked((short)i);
                if (value is short)
                    return value;
                if (value is decimal dec)
                    return checked((short)dec);
                if (value is double d)
                    return checked((short)d);
                if (value is string s && short.TryParse(s, out short sh))
                    return sh;
            }
            else if (dataType == DataTypeNames.BYTE)
            {
                if (value is long l)
                    return checked((byte)l);
                if (value is int i)
                    return checked((byte)i);
                if (value is byte)
                    return value;
                if (value is decimal dec)
                    return checked((byte)dec);
                if (value is double d)
                    return checked((byte)d);
                if (value is string s && byte.TryParse(s, out byte b))
                    return b;
            }
            else if (dataType == DataTypeNames.BOOL)
            {
                if (value is bool)
                    return value;
                if (value is string s && bool.TryParse(s, out bool b))
                    return b;
            }
            else if (dataType == DataTypeNames.DECIMAL)
            {
                if (value is decimal)
                    return value;
                if (value is long l)
                    return (decimal)l;
                if (value is int i)
                    return (decimal)i;
                if (value is double d)
                    return (decimal)d;
                if (value is string s && decimal.TryParse(s, out decimal dec))
                    return dec;
            }
            else if (dataType == DataTypeNames.DOUBLE)
            {
                if (value is double)
                    return value;
                if (value is long l)
                    return (double)l;
                if (value is int i)
                    return (double)i;
                if (value is decimal dec)
                    return (double)dec;
                if (value is string s && double.TryParse(s, out double d))
                    return d;
            }
            else if (dataType == DataTypeNames.SINGLE)
            {
                if (value is float)
                    return value;
                if (value is double d)
                    return (float)d;
                if (value is long l)
                    return (float)l;
                if (value is int i)
                    return (float)i;
                if (value is decimal dec)
                    return (float)dec;
                if (value is string s && float.TryParse(s, out float f))
                    return f;
            }
            else if (dataType == DataTypeNames.GUID)
            {
                if (value is Guid)
                    return value;
                if (value is string s && Guid.TryParse(s, out Guid g))
                    return g;
            }
            else if (dataType == DataTypeNames.DATE_TIME)
            {
                if (value is DateTime)
                    return value;
                if (value is string s && DateTime.TryParse(s, out DateTime dt))
                    return dt;
            }
            else if (dataType == DataTypeNames.STRING)
            {
                if (value is string)
                    return value;
                return value.ToString();
            }

            // If we can't convert safely, keep original
            return value;
        }

    }
}
