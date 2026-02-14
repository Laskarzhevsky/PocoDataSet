using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Centralizes merge-time row indexing behavior.
    /// 
    /// Invariants:
    /// - Merge handlers decide whether a table is "keyed" (PK exists) before indexing.
    /// - When PK exists, the key is the compiled PK tuple value (as used historically by BuildPrimaryKeyIndex).
    /// - When PK does not exist, merge handlers must use destructive replace behavior (no row reconciliation).
    /// </summary>
    public static class RowIndexBuilder
    {
        /// <summary>
        /// Builds an index of rows by their compiled primary key value.
        /// This intentionally mirrors the legacy BuildPrimaryKeyIndex behavior:
        /// - Uses StringComparer.Ordinal
        /// - Keeps the first row for a given key (duplicates are ignored)
        /// - Does not special-case empty keys (empty key is a valid dictionary key)
        /// </summary>
        public static Dictionary<string, IDataRow> BuildRowIndex(IDataTable? dataTable, IReadOnlyList<string> primaryKeyColumnNames)
        {
            Dictionary<string, IDataRow> index = new Dictionary<string, IDataRow>(StringComparer.Ordinal);

            if (dataTable == null)
            {
                return index;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                IDataRow row = dataTable.Rows[i];
                string key = row.CompilePrimaryKeyValue(primaryKeyColumnNames);

                if (!index.ContainsKey(key))
                {
                    index.Add(key, row);
                }
            }

            return index;
        }
    }
}
