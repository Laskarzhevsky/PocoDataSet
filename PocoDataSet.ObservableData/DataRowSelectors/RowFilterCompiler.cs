namespace PocoDataSet.ObservableData
{
    public static class RowFilterCompiler
    {
        // Public entry
        public static IRowFilter Build(string rowFilter)
        {
            return Build(rowFilter, false);
        }

        public static IRowFilter Build(string rowFilter, bool caseSensitive)
        {
            if (string.IsNullOrEmpty(rowFilter))
            {
                return SelectAllFilter.Instance;
            }

            Parser p = new Parser(rowFilter, caseSensitive);
            IRowFilter f = p.ParseExpression();
            p.ExpectEnd();
            return f;
        }
    }
}
