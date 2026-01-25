using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableTests
{
    internal class RowsChangedCounter
    {
        public int Count
        {
            get; private set;
        }

        public void Handler(object? sender, RowsChangedEventArgs e)
        {
            Count++;
        }
    }
}
