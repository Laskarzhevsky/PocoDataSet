using PocoDataSet.IData;

namespace PocoDataSet.ObservableData
{
    internal class OrFilter : IRowFilter
    {
        readonly IRowFilter _a, _b;
        public OrFilter(IRowFilter a, IRowFilter b)
        {
            _a = a;
            _b = b;
        }
        public bool Include(IDataRow row)
        {
            return _a.Include(row) || _b.Include(row);
        }
    }
}
