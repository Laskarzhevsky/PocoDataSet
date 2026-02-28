using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    public interface IObservableDataTable : INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Events
        /// <summary>
        /// Cell changed notification
        /// </summary>
        event EventHandler<DataFieldValueChangedEventArgs>? DataFieldValueChanged;

        /// <summary>
        /// RowsAdded event
        /// </summary>
        event EventHandler<RowsChangedEventArgs>? RowsAdded;

        /// <summary>
        /// RowsRemoved event
        /// </summary>
        event EventHandler<RowsChangedEventArgs>? RowsRemoved;

        /// <summary>
        /// RowStateChanged event
        /// </summary>
        event EventHandler<RowStateChangedEventArgs>? RowStateChanged;
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new row in the inner table and adds it to the observable table,
        /// raising RowsAdded and wiring row change events.
        /// </summary>
        /// <returns>Observable data row</returns>
        IObservableDataRow AddNewRow();

        /// <summary>
        /// Adds observable data row
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        /// <returns>Added observable data row</returns>
        IObservableDataRow AddObservableDataRow(IObservableDataRow observableDataRow);

        /// <summary>
        /// Adds data row by wrapping it into observable data row
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>Observable data row</returns>
        IObservableDataRow AddRow(IDataRow dataRow);

        /// <summary>
        /// Gets flag indicating whether table observable contains specified row
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        /// <returns>True if observable table contains specified row, otherwise false</returns>
        bool ContainsRow(IObservableDataRow observableDataRow);

        /// <summary>
        /// Removes row
        /// IObservableDataTable interface implementation
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        void RemoveRow(IObservableDataRow observableDataRow);

        /// <summary>
        /// Removes row at position specified by row index
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <returns>Removed observable data row </returns>
        IObservableDataRow RemoveRowAt(int rowIndex);

        /// <summary>
        /// Attempts to retrieve the metadata for a column with the specified name
        /// </summary>
        /// <param name="columnName">The name of the column for which to retrieve metadata</param>
        /// <param name="columnMetadata">When this method returns, contains the metadata for the specified column if found, otherwise null</param>
        /// <returns>True if the metadata for the specified column is found; otherwise, false.</returns>
        bool TryGetColumn(string columnName, out IColumnMetadata? columnMetadata);
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets list of columns
        /// </summary>
        IReadOnlyList<IColumnMetadata> Columns
        {
            get;
        }

        /// <summary>
        /// Gets GUID
        /// </summary>
        Guid Guid
        {
            get;
        }

        /// <summary>
        /// Gets inner data table
        /// </summary>
        IDataTable InnerDataTable
        {
            get;
        }

        /// <summary>
        /// Gets list of primary key column names (single source of truth for row identity)
        /// </summary>
        IReadOnlyList<string> PrimaryKeys
        {
            get;
        }

        /// <summary>
        /// Gets rows
        /// </summary>
        IList<IObservableDataRow> Rows
        {
            get;
        }

        /// <summary>
        /// Gets table name
        /// </summary>
        string TableName
        {
            get;
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets the metadata for the column with the specified name.
        /// </summary>
        /// <returns>An instance of IColumnMetadata that contains the metadata for the specified column,
        /// or null if the column does not exist.</returns>
        IColumnMetadata this[string columnName]
        {
            get;
        }
        #endregion
    }
}
