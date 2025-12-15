using System;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Provides RowsChanged event arguments functionality
    /// </summary>
    public class RowsChangedEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <param name="observableDataRow">Observable data row</param>
        public RowsChangedEventArgs(int rowIndex, IObservableDataRow? observableDataRow)
        {
            RowIndex = rowIndex;
            ObservableDataRow = observableDataRow;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets observable data row
        /// </summary>
        public IObservableDataRow? ObservableDataRow
        {
            get; set;
        }

        /// <summary>
        /// Gets row index
        /// </summary>
        public int RowIndex
        {
            get; set;
        }
        #endregion
    }
}
