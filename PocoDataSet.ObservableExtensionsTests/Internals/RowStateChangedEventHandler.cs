using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableExtensionsTests.Internals;

namespace PocoDataSet.ObservableExtensionsTests
{
    /// <summary>
    /// Provides RowStateChanged event handler functionality
    /// </summary>
    class RowStateChangedEventHandler
    {
        #region Data Fields
        /// <summary>
        /// Holds references to handled RowStateChanged event arguments
        /// </summary>
        private readonly System.Collections.Generic.List<RowStateChangedEventHandlerEntry> _handledRowStateChangedEvents = new System.Collections.Generic.List<RowStateChangedEventHandlerEntry>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets event count
        /// </summary>
        /// <returns>Event count</returns>
        public int GetEventCount(IObservableDataRow? observableDataRow = null, DataRowState? oldState = null, DataRowState? newState = null)
        {
            if (observableDataRow == null)
            {
                return _handledRowStateChangedEvents.Count;
            }

            int count = 0;
            for (int i = 0; i < _handledRowStateChangedEvents.Count; i++)
            {
                if (oldState == null && newState == null)
                {
                    if (_handledRowStateChangedEvents[i].ObservableDataRow == observableDataRow)
                    {
                        count++;
                    }
                }
                else if (oldState != null && newState == null)
                {
                    if (_handledRowStateChangedEvents[i].RowStateChangedEventArgs.OldState == oldState)
                    {
                        count++;
                    }
                }
                else if (oldState == null && newState != null)
                {
                    if (_handledRowStateChangedEvents[i].RowStateChangedEventArgs.NewState == newState)
                    {
                        count++;
                    }
                }
                else if (oldState != null && newState != null)
                {
                    if (_handledRowStateChangedEvents[i].RowStateChangedEventArgs.OldState == oldState && _handledRowStateChangedEvents[i].RowStateChangedEventArgs.NewState == newState)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        #endregion

        public bool HasTransition(IObservableDataRow observableDataRow, DataRowState oldState, DataRowState newState)
        {
            for (int i = 0; i < _handledRowStateChangedEvents.Count; i++)
            {
                if (_handledRowStateChangedEvents[i].RowStateChangedEventArgs.OldState == oldState && _handledRowStateChangedEvents[i].RowStateChangedEventArgs.NewState == newState)
                {
                    return true;
                }
            }

            return false;
        }

        #region Evnet Handlers
        /// <summary>
        /// Handles RowStateChanged Event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        public void Handler(object? sender, RowStateChangedEventArgs e)
        {
            if (sender == null) 
            {
                return;
            }

            _handledRowStateChangedEvents.Add(new RowStateChangedEventHandlerEntry((IObservableDataRow)sender, e));
        }
        #endregion
    }
}
