using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensionsTests.Internals
{
    /// <summary>
    /// Provides data field value changed event handler entry functionality
    /// </summary>
    class DataFieldValueChangedEventHandlerEntry
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        /// <param name="dataFieldValueChangedEventArgs">Data field value changed event args</param>
        public DataFieldValueChangedEventHandlerEntry(object? sender, DataFieldValueChangedEventArgs dataFieldValueChangedEventArgs)
        {
            if (sender != null)
            {
                ObservableDataRow = (IObservableDataRow)sender;
            }

            DataFieldValueChangedEventArgs = dataFieldValueChangedEventArgs;
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
        /// Gets or sets data field value changed event args
        /// </summary>
        public DataFieldValueChangedEventArgs DataFieldValueChangedEventArgs
        {
            get; set;
        }
        #endregion
    }
}
