using System;

namespace PocoDataSet.ObservableExtensionsTests
{
    /// <summary>
    /// Counts PropertyChanged notifications for a specific property name.
    /// </summary>
    public sealed class PropertyChangedCounter
    {
        readonly string _propertyName;

        public PropertyChangedCounter(string propertyName)
        {
            _propertyName = propertyName;
        }

        public int Count
        {
            get; private set;
        }

        public void Handler(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            if (string.Equals(e.PropertyName, _propertyName, StringComparison.Ordinal))
            {
                Count++;
            }
        }
    }
}
