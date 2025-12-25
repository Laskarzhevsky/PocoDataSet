using System;

using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Provides RowStateChanged event arguments functionality
    /// </summary>
    public class RowStateChangedEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="oldState">Old state</param>
        /// <param name="newState">New state</param>
        /// <param name="requestor">Object which requests update</param>
        public RowStateChangedEventArgs(DataRowState oldState, DataRowState newState, object? requestor)
        {
            OldState = oldState;
            NewState = newState;
            Requestor = requestor;
        }
        #endregion

        /// <summary>
        /// Gets old state
        /// </summary>
        public DataRowState OldState
        {
            get; private set;
        }

        /// <summary>
        /// Gets new state
        /// </summary>
        public DataRowState NewState
        {
            get; private set;
        }

        /// <summary>
        /// Gets object which requests update
        /// </summary>
        public object? Requestor
        {
            get; private set;
        }
    }
}
