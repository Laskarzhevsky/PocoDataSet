using System;
using System.Collections.Generic;
using System.Globalization;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableData
{
    /// <summary>
    /// Provides composite row comparer fuctionality
    /// </summary>
    internal class CompositeRowComparer : IComparer<IObservableDataRow>
    {
        readonly List<SortKey> _keys;
        public CompositeRowComparer(List<SortKey> keys)
        {
            _keys = keys;
        }

        public int Compare(IObservableDataRow x, IObservableDataRow y)
        {
            int i = 0;
            while (i < _keys.Count)
            {
                SortKey k = _keys[i];
                int c = CompareByColumn(x, y, k.ColumnName);
                if (c != 0)
                {
                    if (k.Ascending)
                        return c;
                    return -c;
                }
                i = i + 1;
            }
            return 0;
        }

        static int CompareByColumn(IObservableDataRow a, IObservableDataRow b, string column)
        {
            object va, vb;

            bool oka = RowFilterCompilerBuild_Get(a.InnerDataRow, column, out va); // supports ParentPath too
            bool okb = RowFilterCompilerBuild_Get(b.InnerDataRow, column, out vb);

            if (!oka && !okb)
                return 0;
            if (!oka)
                return -1;
            if (!okb)
                return 1;

            if (va == null && vb == null)
                return 0;
            if (va == null)
                return -1;
            if (vb == null)
                return 1;

            string sa = Convert.ToString(va);
            string sb = Convert.ToString(vb);

            if (sa == null && sb == null)
                return 0;
            if (sa == null)
                return -1;
            if (sb == null)
                return 1;

            double da, db;
            bool na = double.TryParse(sa, NumberStyles.Any, CultureInfo.InvariantCulture, out da);
            bool nb = double.TryParse(sb, NumberStyles.Any, CultureInfo.InvariantCulture, out db);
            if (na && nb)
            {
                if (da < db)
                    return -1;
                if (da > db)
                    return 1;
                return 0;
            }

            return string.Compare(sa, sb, StringComparison.Ordinal);
        }

        // small bridge so we can reuse the same virtual column logic as the filter
        static bool RowFilterCompilerBuild_Get(IDataRow row, string column, out object? value)
        {
            return row.TryGetValue(column, out value);
        }
    }
}
