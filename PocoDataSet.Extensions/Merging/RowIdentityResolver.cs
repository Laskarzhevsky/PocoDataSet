using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Centralized row identity resolution (PK-first, then __ClientKey).
    /// </summary>
    public static class RowIdentityResolver
    {
        #region Public Methods
        public static bool TryGetRowKey(IDataRow? row, IReadOnlyList<string> primaryKeyColumnNames, out string rowKey)
        {
            rowKey = string.Empty;

            if (row == null)
            {
                return false;
            }

            bool hasPrimaryKey = primaryKeyColumnNames != null && primaryKeyColumnNames.Count > 0;
            if (hasPrimaryKey)
            {
                string pkValue = CompilePrimaryKeyValue(row, primaryKeyColumnNames);
                rowKey = pkValue;
                return true;
            }

            Guid clientKey;
            bool hasClientKey = TryGetClientKey(row, out clientKey);
            if (hasClientKey)
            {
                rowKey = clientKey.ToString("D", CultureInfo.InvariantCulture);
                return true;
            }

            return false;
        }

        public static bool TryGetPrimaryKeyValue(IDataRow? row, IReadOnlyList<string> primaryKeyColumnNames, out string primaryKeyValue)
        {
            primaryKeyValue = string.Empty;

            if (row == null)
            {
                return false;
            }

            if (primaryKeyColumnNames == null || primaryKeyColumnNames.Count == 0)
            {
                return false;
            }

            primaryKeyValue = CompilePrimaryKeyValue(row, primaryKeyColumnNames);
            return true;
        }

        public static bool TryGetClientKey(IDataRow? row, out Guid clientKey)
        {
            clientKey = Guid.Empty;

            if (row == null)
            {
                return false;
            }

            object? raw;
            bool found = row.TryGetValue(SpecialColumnNames.CLIENT_KEY, out raw);
            if (!found)
            {
                return false;
            }

            if (raw == null)
            {
                return false;
            }

            if (raw is Guid guid)
            {
                clientKey = guid;
                return true;
            }

            string? s = raw as string;
            if (!string.IsNullOrWhiteSpace(s))
            {
                Guid parsed;
                bool ok = Guid.TryParse(s, out parsed);
                if (ok)
                {
                    clientKey = parsed;
                    return true;
                }
            }

            return false;
        }

        public static Dictionary<Guid, IDataRow> BuildClientKeyIndex(IEnumerable<IDataRow> rows)
        {
            Dictionary<Guid, IDataRow> index = new Dictionary<Guid, IDataRow>();

            foreach (IDataRow row in rows)
            {
                Guid clientKey;
                bool hasClientKey = TryGetClientKey(row, out clientKey);
                if (!hasClientKey)
                {
                    continue;
                }

                // First wins (matches prior behavior in BuildPrimaryKeyIndex)
                if (!index.ContainsKey(clientKey))
                {
                    index.Add(clientKey, row);
                }
            }

            return index;
        }
        #endregion

        #region Methods
        static string CompilePrimaryKeyValue(IDataRow dataRow, IReadOnlyList<string> primaryKeyColumnNames)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < primaryKeyColumnNames.Count; i++)
            {
                string columnName = primaryKeyColumnNames[i];

                object? raw;
                bool found = dataRow.TryGetValue(columnName, out raw);
                if (!found)
                {
                    // Keep old behavior: schema says PK column exists; missing is a bug.
                    throw new KeyNotFoundException(columnName);
                }

                string stringRepresentationOfValue = GetStringRepresentationOf(raw);

                if (i > 0)
                {
                    stringBuilder.Append('|');
                }

                stringBuilder.Append(stringRepresentationOfValue.Length.ToString(CultureInfo.InvariantCulture));
                stringBuilder.Append('#');
                stringBuilder.Append(stringRepresentationOfValue);
            }

            return stringBuilder.ToString();
        }

        static string GetStringRepresentationOf(object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (Convert.IsDBNull(value))
            {
                return string.Empty;
            }

            IFormattable? formattable = value as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;
            }

            return value.ToString() ?? string.Empty;
        }
        #endregion
    }
}
