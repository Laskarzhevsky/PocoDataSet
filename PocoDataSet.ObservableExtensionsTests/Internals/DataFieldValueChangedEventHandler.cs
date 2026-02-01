using PocoDataSet.IObservableData;
using PocoDataSet.ObservableExtensionsTests.Internals;

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
        private readonly System.Collections.Generic.List<DataFieldValueChangedEventHandlerEntry> _handledDataFieldValueChangedEvents = new System.Collections.Generic.List<DataFieldValueChangedEventHandlerEntry>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets event count
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>Event count</returns>
        public int GetEventCount(IObservableDataRow observableDataRow, string? columnName = null)
        {
            int count = 0;
            for (int i = 0; i < _handledDataFieldValueChangedEvents.Count; i++)
            {
                IObservableDataRow? handledObservableDataRow = _handledDataFieldValueChangedEvents[i].ObservableDataRow;
                if (handledObservableDataRow == observableDataRow)
                {
                    if (string.IsNullOrEmpty(columnName))
                    {
                        count++;
                    }
                    else
                    {
                        if (_handledDataFieldValueChangedEvents[i].DataFieldValueChangedEventArgs.ColumnName == columnName)
                        {
                            count++;
                        }
                    }
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
            _handledDataFieldValueChangedEvents.Add(new DataFieldValueChangedEventHandlerEntry((IObservableDataRow)sender, e));
        }
        #endregion
    }
}
