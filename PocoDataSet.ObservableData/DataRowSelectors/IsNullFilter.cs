using System;

using PocoDataSet.IData;

namespace PocoDataSet.ObservableData
{
    internal class IsNullFilter : IRowFilter
    {
        readonly string _col;
        readonly bool _testIsNull; // true=IS NULL, false=IS NOT NULL
        public IsNullFilter(string col, bool testIsNull)
        {
            _col = col;
            _testIsNull = testIsNull;
        }
        public bool Include(IDataRow row)
        {
            object v;
            bool ok = row.TryGetValue(_col, out v);
            bool isNull = !ok || v == null || Convert.ToString(v) == null;
            if (_testIsNull)
                return isNull;
            return !isNull;
        }
    }
}
