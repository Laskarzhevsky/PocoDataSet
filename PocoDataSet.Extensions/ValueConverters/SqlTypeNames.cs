using System;

namespace PocoDataSet.Extensions
{
    public static class SqlTypeNames
    {
        public static bool IsDateLike(string? dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType))
            {
                return false;
            }

            string s = dataType.Trim();

            if (string.Equals(s, "date", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (string.Equals(s, "datetime", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (string.Equals(s, "datetime2", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (string.Equals(s, "smalldatetime", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
