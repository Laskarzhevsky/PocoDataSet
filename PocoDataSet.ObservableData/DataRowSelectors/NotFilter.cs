using PocoDataSet.IData;

namespace PocoDataSet.ObservableData
{
    internal class NotFilter : IRowFilter
    {
        readonly IRowFilter _inner;
        public NotFilter(IRowFilter inner)
        {
            _inner = inner;
        }
        public bool Include(IDataRow row)
        {
            return !_inner.Include(row);
        }
    }
}
