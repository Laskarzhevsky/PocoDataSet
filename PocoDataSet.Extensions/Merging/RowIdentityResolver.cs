using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Centralizes row identity resolution for merge operations.
    /// Primary rule: if schema defines primary keys, use them.
    /// Fallback: use <see cref="SpecialColumnNames.CLIENT_KEY"/> when present (changeset correlation).
    /// </summary>
    public static class RowIdentityResolver
    {
        /// <summary>
        /// Tries to compute the compiled primary key value for the given row.
        /// </summary>
        public static bool TryGetPrimaryKeyValue(IDataRow row, IReadOnlyList<string> primaryKeyColumnNames, out string primaryKeyValue)
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

            string compiled = row.CompilePrimaryKeyValue(primaryKeyColumnNames);
            if (string.IsNullOrEmpty(compiled))
            {
                return false;
            }

            primaryKeyValue = compiled;
            return true;
        }

        /// <summary>
        /// Tries to read the client key from the given row.
        /// </summary>
        public static bool TryGetClientKey(IDataRow row, out Guid clientKey)
        {
            clientKey = Guid.Empty;

            if (row == null)
            {
                return false;
            }

            if (!row.ContainsKey(SpecialColumnNames.CLIENT_KEY))
            {
                return false;
            }

            object? value = row[SpecialColumnNames.CLIENT_KEY];
            if (value == null)
            {
                return false;
            }

            if (value is Guid)
            {
                clientKey = (Guid)value;
                return clientKey != Guid.Empty;
            }

            return false;
        }

        /// <summary>
        /// Builds an index of rows by client key. Rows without a client key are skipped.
        /// </summary>
        public static Dictionary<Guid, IDataRow> BuildClientKeyIndex(IDataTable dataTable)
        {
            Dictionary<Guid, IDataRow> index = new Dictionary<Guid, IDataRow>();

            if (dataTable == null)
            {
                return index;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                IDataRow row = dataTable.Rows[i];

                Guid clientKey;
                if (TryGetClientKey(row, out clientKey))
                {
                    if (!index.ContainsKey(clientKey))
                    {
                        index.Add(clientKey, row);
                    }
                }
            }

            return index;
        }

        /// <summary>
        /// Tries to build a deterministic row key string (PK first, then client key).
        /// </summary>
        public static bool TryGetRowKey(IDataRow row, IReadOnlyList<string> primaryKeyColumnNames, out string rowKey)
        {
            rowKey = string.Empty;

            string pk;
            if (TryGetPrimaryKeyValue(row, primaryKeyColumnNames, out pk))
            {
                rowKey = "PK:" + pk;
                return true;
            }

            Guid clientKey;
            if (TryGetClientKey(row, out clientKey))
            {
                rowKey = "CK:" + clientKey.ToString("D");
                return true;
            }

            return false;
        }
    }
}
