using PocoDataSet.IObservableData;

using System;

namespace PocoDataSet.ObservableExtensionsTests
{
    /// <summary>
    /// Counts DataFieldValueChanged notifications for a specific column (or all columns if not specified).
    /// </summary>
    public sealed class DataFieldValueChangedCounter
    {
        readonly string? _columnName;

        public DataFieldValueChangedCounter()
        {
        }

        public DataFieldValueChangedCounter(string columnName)
        {
            _columnName = columnName;
        }

        public int Count
        {
            get; private set;
        }

        public void Handler(object? sender, DataFieldValueChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            if (_columnName == null || string.Equals(e.ColumnName, _columnName, StringComparison.Ordinal))
            {
                Count++;
            }
        }
    }
}
