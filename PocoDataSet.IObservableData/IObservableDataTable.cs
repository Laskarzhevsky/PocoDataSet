using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    public interface IObservableDataTable
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
        /// Removes row at position specified by row index
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <returns>Removed observable data row </returns>
        IObservableDataRow RemoveRowAt(int rowIndex);
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets list of columns
        /// </summary>
        IList<IColumnMetadata> Columns
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
    }
}
