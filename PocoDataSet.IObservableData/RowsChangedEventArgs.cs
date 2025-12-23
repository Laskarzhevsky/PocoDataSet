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
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="observableDataRow">Observable data row</param>
        public RowsChangedEventArgs(string tableName, int rowIndex, IObservableDataRow? observableDataRow)
        {
            RowIndex = rowIndex;
            ObservableDataRow = observableDataRow;
            TableName = tableName;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets observable data row
        /// </summary>
        public IObservableDataRow? ObservableDataRow
        {
            get; private set;
        }

        /// <summary>
        /// Gets row index
        /// </summary>
        public int RowIndex
        {
            get; private set;
        }

        /// <summary>
        /// Gets table name
        /// </summary>
        public string TableName
        {
            get; private set;
        }
        #endregion
    }
}
