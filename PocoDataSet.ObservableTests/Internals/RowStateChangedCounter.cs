using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableTests
{
    internal sealed class RowStateChangedCounter
    {
        public int Count
        {
            get; private set;
        }

        public void Handler(object? sender, RowStateChangedEventArgs e)
        {
            Count++;
        }
    }
}
