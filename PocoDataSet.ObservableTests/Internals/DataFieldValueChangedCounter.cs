using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableTests
{
    internal class DataFieldValueChangedCounter
    {
        public int Count
        {
            get; private set;
        }

        public void Handler(object? sender, DataFieldValueChangedEventArgs e)
        {
            Count++;
        }
    }
}
