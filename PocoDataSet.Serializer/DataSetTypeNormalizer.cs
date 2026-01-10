using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Serializer
{
    internal static class DataSetTypeNormalizer
    {
        internal static void NormalizeDataSet(IDataSet? dataSet)
        {
            if (dataSet == null)
            {
                return;
            }

            foreach (KeyValuePair<string, IDataTable> tablePair in dataSet.Tables)
            {
                IDataTable table = tablePair.Value;
                NormalizeTable(table);
            }
        }

        static void NormalizeTable(IDataTable table)
        {
            if (table == null)
            {
                return;
            }

            for (int r = 0; r < table.Rows.Count; r++)
            {
                IDataRow row = table.Rows[r];
                NormalizeRow(table, row);
            }
        }

        static void NormalizeRow(IDataTable table, IDataRow row)
        {
            if (row == null)
            {
                return;
            }

            // IMPORTANT:
            // - Avoid using the row indexer if you don't want to affect DataRowState snapshots.
            // - We normalize by writing to ValuesJson / OriginalValuesJson directly if row is a DataRow.
            DataRow? concreteRow = row as DataRow;
            if (concreteRow == null)
            {
                return;
            }

            Dictionary<string, object?> values = concreteRow.ValuesJson;
            Dictionary<string, object?>? original = concreteRow.OriginalValuesJson;

            // IMPORTANT (Forward compatibility semantics):
            // We DO NOT remove unknown keys that appear in incoming JSON.
            // Example: schema evolves and a newer payload contains "Ghost" while the
            // current client schema contains only Id+Name.
            //
            // Keeping unknown keys allows:
            // - older clients to read newer payloads without data loss
            // - "floating" patch payloads to carry extra fields safely
            //
            // Unknown keys are simply left untouched (no type normalization), and are
            // naturally ignored by writers (EF/SQL) that operate only on known columns.
            // NOTE: Do not remove unknown keys (see comment above).

            for (int c = 0; c < table.Columns.Count; c++)
            {
                IColumnMetadata col = table.Columns[c];
                string name = col.ColumnName;

                // Floating (sparse) rows must preserve the difference between:
                // - missing key => not provided
                // - key present with null => explicitly set to null
                // Therefore: normalize types only for keys that exist; do NOT add missing keys.
                object? v;
                if (values.TryGetValue(name, out v))
                {
                    values[name] = ValueNormalization.ConvertValueToColumnType(col.DataType, v);
                }

                if (original != null)
                {
                    object? ov;
                    if (original.TryGetValue(name, out ov))
                    {
                        original[name] = ValueNormalization.ConvertValueToColumnType(col.DataType, ov);
                    }
                }
            }
        }

        // Intentionally no "RemoveUnknownKeys" helper in forward-compatible mode.
    }
}
