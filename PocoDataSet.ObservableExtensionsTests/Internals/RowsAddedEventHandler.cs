using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensionsTests
{
    /// <summary>
    /// Provides RowsAdded event handler functionality
    /// </summary>
    class RowsAddedEventHandler
    {
        #region Data Fields
        /// <summary>
        /// Holds references to handled RowsChanged event arguments
        /// </summary>
        private readonly System.Collections.Generic.List<RowsChangedEventArgs> _handledRowsAddedEvents = new System.Collections.Generic.List<RowsChangedEventArgs>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets event count
        /// </summary>
        /// <returns>Event count</returns>
        public int GetEventCount(IObservableDataRow? observableDataRow = null)
        {
            if (observableDataRow == null)
            {
                return _handledRowsAddedEvents.Count;
            }

            int count = 0;
            for (int i = 0; i < _handledRowsAddedEvents.Count; i++)
            {
                if (_handledRowsAddedEvents[i].ObservableDataRow == observableDataRow)
                {
                    count++;
                }
            }

            return count;
        }
        #endregion

        #region Evnet Handlers
        /// <summary>
        /// Handles RowStateChanged Event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        public void Handler(object? sender, RowsChangedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            _handledRowsAddedEvents.Add(e);
        }
        #endregion
    }
}
