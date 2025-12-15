using PocoDataSet.IData;

namespace PocoDataSet.ObservableData
{
    internal class AndFilter : IRowFilter
    {
        readonly IRowFilter _a, _b;
        public AndFilter(IRowFilter a, IRowFilter b)
        {
            _a = a;
            _b = b;
        }
        public bool Include(IDataRow row)
        {
            return _a.Include(row) && _b.Include(row);
        }
    }
}
