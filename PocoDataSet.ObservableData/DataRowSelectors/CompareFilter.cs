using System;
using System.Globalization;

using PocoDataSet.IData;

namespace PocoDataSet.ObservableData
{
    internal class CompareFilter : IRowFilter
    {
        readonly string _col;
        readonly bool _isEq;
        readonly object _value;
        bool _caseSensitive;

        public CompareFilter(string col, bool isEq, object value, bool caseSensitive)
        {
            _col = col;
            _isEq = isEq;
            _value = value;
            _caseSensitive = caseSensitive;
        }

        public bool Include(IDataRow row)
        {
            object v;
            bool ok = row.TryGetValue(_col, out v);
            if (!ok)
                return false;
            string sLeft = v == null ? null : Convert.ToString(v);
            string sRight = _value == null ? null : Convert.ToString(_value);

            if (sLeft == null || sRight == null)
            {
                if (_isEq)
                    return sLeft == sRight;
                return sLeft != sRight;
            }

            // try numeric compare first
            double dl, dr;
            bool nl = double.TryParse(sLeft, NumberStyles.Any, CultureInfo.InvariantCulture, out dl);
            bool nr = double.TryParse(sRight, NumberStyles.Any, CultureInfo.InvariantCulture, out dr);
            if (nl && nr)
            {
                bool eq = dl == dr;
                if (_isEq)
                    return eq;
                else
                    return !eq;
            }

            StringComparison cmp;
            if (_caseSensitive)
                cmp = StringComparison.Ordinal;
            else
                cmp = StringComparison.OrdinalIgnoreCase;

            int c = string.Compare(sLeft, sRight, cmp);
            bool isEq = (c == 0);
            if (_isEq)
                return isEq;
            return !isEq;
        }
    }
}
