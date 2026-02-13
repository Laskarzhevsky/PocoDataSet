using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    public interface IObservableDataView : IDisposable, IAsyncDisposable
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
        #endregion

        #region Methods
        /// <summary>
        /// Adds data row
        /// </summary>
        /// <param name="dataRow">Observable data row</param>
        void AddRow(IObservableDataRow observableDataRow);

        /// <summary>
        /// Removes row at specified index
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        void RemoveRow(int rowIndex);
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
        /// Gets inner observable data table
        /// </summary>
        IObservableDataTable InnerObservableDataTable
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
        /// Gets requestor name
        /// </summary>
        string? RequestorName
        {
            get;
        }

        /// <summary>
        /// Gets view name consisting of concatenation of inner observable data table name with requestor name
        /// </summary>
        string ViewName
        {
            get;
        }
        #endregion
    }
}
