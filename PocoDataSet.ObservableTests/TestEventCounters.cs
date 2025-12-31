using System;

using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableTests
{
    internal sealed class DataFieldValueChangedCounter
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

    internal sealed class RowsChangedCounter
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
