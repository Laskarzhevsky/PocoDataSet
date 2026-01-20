using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensionsTests
{
    /// <summary>
    /// Provides RowsRemoved event handler functionality
    /// </summary>
    class RowsRemovedEventHandler
    {
        #region Data Fields
        /// <summary>
        /// Holds references to handled RowsChanged event arguments
        /// </summary>
        private readonly System.Collections.Generic.List<RowsChangedEventArgs> _handledRowsRemovedEvents = new System.Collections.Generic.List<RowsChangedEventArgs>();
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
                return _handledRowsRemovedEvents.Count;
            }

            int count = 0;
            for (int i = 0; i < _handledRowsRemovedEvents.Count; i++)
            {
                if (_handledRowsRemovedEvents[i].ObservableDataRow == observableDataRow)
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

            _handledRowsRemovedEvents.Add(e);
        }
        #endregion
    }
}
