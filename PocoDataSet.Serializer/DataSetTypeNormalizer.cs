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

            for (int c = 0; c < table.Columns.Count; c++)
            {
                IColumnMetadata col = table.Columns[c];
                string name = col.ColumnName;

                object? v;
                if (values.TryGetValue(name, out v))
                {
                    values[name] = ValueNormalization.ConvertValueToColumnType(col.DataType, v);
                }
                else
                {
                    // keep invariant: key exists
                    values[name] = null;
                }

                if (original != null)
                {
                    object? ov;
                    if (original.TryGetValue(name, out ov))
                    {
                        original[name] = ValueNormalization.ConvertValueToColumnType(col.DataType, ov);
                    }
                    else
                    {
                        original[name] = null;
                    }
                }
            }
        }
    }
}
