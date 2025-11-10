using System;

namespace PocoDataSet.Extensions
{
    public static class TypeKind
    {
        public static Type GetUnderlyingOrSelf(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type? u = Nullable.GetUnderlyingType(type);
            if (u == null)
            {
                return type;
            }
            else
            {
                return u;
            }
        }

        public static bool IsString(Type type)
        {
            Type u = GetUnderlyingOrSelf(type);
            return u == typeof(string);
        }

        public static bool IsGuid(Type type)
        {
            Type u = GetUnderlyingOrSelf(type);
            return u == typeof(Guid);
        }

        public static bool IsEnum(Type type)
        {
            Type u = GetUnderlyingOrSelf(type);
            return u.IsEnum;
        }

        public static bool IsBoolean(Type type)
        {
            Type u = GetUnderlyingOrSelf(type);
            return u == typeof(bool);
        }

        public static bool IsDateTime(Type type)
        {
            Type u = GetUnderlyingOrSelf(type);
            return u == typeof(DateTime);
        }

        public static bool IsConvertible(Type type)
        {
            Type u = GetUnderlyingOrSelf(type);
            return typeof(IConvertible).IsAssignableFrom(u);
        }
    }
}
