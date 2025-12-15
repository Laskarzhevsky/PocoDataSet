using System;
using System.Collections.Generic;

using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableData
{
    /// <summary>
    /// Provides sort compiler fuctionality
    /// </summary>
    public static class SortCompiler
    {
        public static IComparer<IObservableDataRow> Build(string? sort)
        {
            if (string.IsNullOrEmpty(sort))
            {
                return new DefaultRowComparer(); // keep your legacy default
            }
            List<SortKey> keys = Parse(sort);
            return new CompositeRowComparer(keys);
        }

        static List<SortKey> Parse(string sort)
        {
            // Format: Col [ASC|DESC] [, Col2 [ASC|DESC] ...]
            List<SortKey> list = new List<SortKey>();
            string[] parts = sort.Split(',');
            int i = 0;
            while (i < parts.Length)
            {
                string part = parts[i].Trim();
                if (part.Length == 0)
                {
                    i = i + 1;
                    continue;
                }

                string[] bits = part.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (bits.Length == 0)
                {
                    i = i + 1;
                    continue;
                }

                SortKey k = new SortKey();
                k.ColumnName = bits[0];
                k.Ascending = true;

                if (bits.Length >= 2)
                {
                    string d = bits[1].ToUpperInvariant();
                    if (d == "DESC")
                        k.Ascending = false;
                    else
                        k.Ascending = true; // ASC or anything else treated as ASC
                }

                list.Add(k);
                i = i + 1;
            }

            if (list.Count == 0)
            {
                // fallback: default
                list.Add(new SortKey { ColumnName = "Order", Ascending = true });
                list.Add(new SortKey { ColumnName = "DisplayText", Ascending = true });
                list.Add(new SortKey { ColumnName = "TransitionName", Ascending = true });
            }

            return list;
        }
    }
}
