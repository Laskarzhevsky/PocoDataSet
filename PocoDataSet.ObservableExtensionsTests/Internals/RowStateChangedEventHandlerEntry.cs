using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensionsTests.Internals
{
    /// <summary>
    /// Provides row state changed event handler entry functionality
    /// </summary>
    class RowStateChangedEventHandlerEntry
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        /// <param name="rowStateChangedEventArgs">Row state changed event args</param>
        public RowStateChangedEventHandlerEntry(object? sender, RowStateChangedEventArgs rowStateChangedEventArgs)
        {
            if (sender != null)
            {
                ObservableDataRow = (IObservableDataRow)sender;
            }

            RowStateChangedEventArgs = rowStateChangedEventArgs;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets observable data row
        /// </summary>
        public IObservableDataRow? ObservableDataRow
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets row state changed event args
        /// </summary>
        public RowStateChangedEventArgs RowStateChangedEventArgs
        {
            get; set;
        }
        #endregion
    }
}
