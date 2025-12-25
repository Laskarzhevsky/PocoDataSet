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
        /// Adds data row
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>Observable data row</returns>
        IObservableDataRow AddRow(IDataRow dataRow);

        /// <summary>
        /// Removes row
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <returns>Removed observable data row </returns>
        IObservableDataRow RemoveRow(int rowIndex);
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
