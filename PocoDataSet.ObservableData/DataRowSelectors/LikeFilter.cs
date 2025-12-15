using System;

using PocoDataSet.IData;

namespace PocoDataSet.ObservableData
{
    internal class LikeFilter : IRowFilter
    {
        readonly string _col, _pattern;
        bool _caseSensitive;
        public LikeFilter(string col, string pattern, bool caseSensitive)
        {
            _col = col;
            _pattern = pattern ?? string.Empty;
            _caseSensitive = caseSensitive;
        }
        public bool Include(IDataRow row)
        {
            object v;
            bool ok = row.TryGetValue(_col, out v);
            if (!ok || v == null)
                return false;
            string s = Convert.ToString(v);
            if (s == null)
                return false;
            if (_caseSensitive)
            {
                return LikeMatch(s, _pattern);
            }
            else
            {
                string ts = s.ToUpperInvariant();
                string ps = _pattern.ToUpperInvariant();
                return LikeMatch(ts, ps);
            }
        }

        static bool LikeMatch(string text, string pattern)
        {
            // simple LIKE: % = any-many, _ = any-one (case sensitive, Ordinal)
            int ti = 0, pi = 0;
            int tlen = text.Length, plen = pattern.Length;
            int starText = -1, starPat = -1;

            while (ti < tlen)
            {
                if (pi < plen && (pattern[pi] == '_' || pattern[pi] == text[ti]))
                {
                    ti++;
                    pi++;
                    continue;
                }
                if (pi < plen && pattern[pi] == '%')
                {
                    starPat = pi++;
                    starText = ti;
                    continue;
                }
                if (starPat >= 0)
                {
                    pi = starPat + 1;
                    starText++;
                    ti = starText;
                    continue;
                }
                return false;
            }
            while (pi < plen && pattern[pi] == '%')
                pi++;
            return pi == plen;
        }
    }
}
