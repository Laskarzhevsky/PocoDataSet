using System.Collections.Generic;

using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableData
{
    /// <summary>
    /// Provides default row comparer fuctionality
    /// </summary>
    internal class DefaultRowComparer : IComparer<IObservableDataRow>
    {
        public int Compare(IObservableDataRow a, IObservableDataRow b)
        {
            int c = CompareByColumn(a, b, "Order");
            if (c != 0)
                return c;
            c = CompareByColumn(a, b, "DisplayText");
            if (c != 0)
                return c;
            return CompareByColumn(a, b, "TransitionName");
        }

        static int CompareByColumn(IObservableDataRow a, IObservableDataRow b, string column)
        {
            return new CompositeRowComparer(new List<SortKey> { new SortKey { ColumnName = column, Ascending = true } }).Compare(a, b);
        }
    }
}
