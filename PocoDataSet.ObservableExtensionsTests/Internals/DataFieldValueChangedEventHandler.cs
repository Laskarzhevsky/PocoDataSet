using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensionsTests
{
    /// <summary>
    /// Provides DataFieldValueChanged event handler functionality
    /// </summary>
    class DataFieldValueChangedEventHandler
    {
        #region Data Fields
        /// <summary>
        /// Holds references to handled DataFieldValueChanged event arguments
        /// </summary>
        private readonly System.Collections.Generic.List<DataFieldValueChangedEventArgs> _handledDataFieldValueChangedEvents = new System.Collections.Generic.List<DataFieldValueChangedEventArgs>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets event count
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>Event count</returns>
        public int GetEventCount(string? columnName = null)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return _handledDataFieldValueChangedEvents.Count;
            }

            int count = 0;
            for (int i = 0; i < _handledDataFieldValueChangedEvents.Count; i++)
            {
                if (_handledDataFieldValueChangedEvents[i].ColumnName == columnName)
                {
                    count++;
                }
            }

            return count;
        }
        #endregion

        #region Evnet Handlers
        /// <summary>
        /// Handles DataFieldValueChanged Event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        public void Handle(object? sender, DataFieldValueChangedEventArgs e)
        {
            _handledDataFieldValueChangedEvents.Add(e);
        }
        #endregion
    }
}
