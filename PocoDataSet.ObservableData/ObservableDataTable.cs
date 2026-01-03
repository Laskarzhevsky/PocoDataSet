using System;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableData
{
    /// <summary>
    /// Provides observable data table fuctionality
    /// </summary>
    public class ObservableDataTable : IObservableDataTable
    {
        #region Events
        /// <summary>
        /// DataFieldValueChanged event
        /// IObservableDataTable interface implementation
        /// </summary>
        public event EventHandler<DataFieldValueChangedEventArgs>? DataFieldValueChanged;

        /// <summary>
        /// RowsAdded event
        /// IObservableDataTable interface implementation
        /// </summary>
        public event EventHandler<RowsChangedEventArgs>? RowsAdded;

        /// <summary>
        /// RowsRemoved event
        /// IObservableDataTable interface implementation
        /// </summary>
        public event EventHandler<RowsChangedEventArgs>? RowsRemoved;

        /// <summary>
        /// RowStateChanged event
        /// IObservableDataTable interface implementation
        /// </summary>
        public event EventHandler<RowStateChangedEventArgs>? RowStateChanged;
        #endregion

        #region Data Fields
        /// <summary>
        /// Holds reference to inner data table
        /// </summary>
        readonly IDataTable _innerDataTable;

        /// <summary>
        /// Holds reference to list of observable data rows
        /// </summary>
        readonly List<IObservableDataRow> _observableDataRows = new List<IObservableDataRow>();
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="dataTable">Data table</param>
        public ObservableDataTable(IDataTable dataTable)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable));
            }

            _innerDataTable = dataTable;
            CreateObservableRows();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a new row in the inner table and adds it to the observable table,
        /// raising RowsAdded and wiring row change events.
        /// IObservableDataTable interface implementation
        /// </summary>
        /// <returns>Observable data row</returns>
        public IObservableDataRow AddNewRow()
        {
            IDataRow dataRow = _innerDataTable.AddNewRow();
            return AddRow(dataRow);
        }

        /// <summary>
        /// Adds observable data row
        /// IObservableDataTable interface implementation
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        /// <returns>Added observable data row</returns>
        public IObservableDataRow AddObservableDataRow(IObservableDataRow observableDataRow)
        {
            observableDataRow.DataFieldValueChanged += ObservableDataRow_DataFieldValueChanged;
            observableDataRow.RowStateChanged += ObservableDataRow_RowStateChanged;
            _observableDataRows.Add(observableDataRow);
            RaiseRowAddedEvent(_observableDataRows.Count - 1, observableDataRow);

            return observableDataRow;
        }

        /// <summary>
        /// Adds data row
        /// IObservableDataTable interface implementation
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>Observable data row</returns>
        public IObservableDataRow AddRow(IDataRow dataRow)
        {
            // Keep inner table consistent
            if (!_innerDataTable.ContainsRow(dataRow))
            {
                _innerDataTable.AddRow(dataRow);
            }

            IObservableDataRow observableDataRow = new ObservableDataRow(dataRow);

            return AddObservableDataRow(observableDataRow);
        }

        /// <summary>
        /// Removes row
        /// IObservableDataTable interface implementation
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <returns>Removed observable data row </returns>
        public IObservableDataRow RemoveRow(int rowIndex)
        {
            IObservableDataRow observableDataRow = _observableDataRows[rowIndex];
            observableDataRow.DataFieldValueChanged -= ObservableDataRow_DataFieldValueChanged;
            observableDataRow.RowStateChanged -= ObservableDataRow_RowStateChanged;

            // Keep inner table consistent
            if (rowIndex >= 0 && rowIndex < _innerDataTable.Rows.Count)
            {
                _innerDataTable.RemoveRowAt(rowIndex);
            }

            _observableDataRows.RemoveAt(rowIndex);
            RaiseRowRemovedEvent(rowIndex, observableDataRow);

            return observableDataRow;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets list of columns
        /// IObservableDataTable interface implementation
        /// </summary>
        public IList<IColumnMetadata> Columns
        {
            get
            {
                return _innerDataTable.Columns;
            }
        }

        /// <summary>
        /// Gets GUID
        /// IObservableDataTable interface implementation
        /// </summary>
        public Guid Guid
        {
            get; set;
        } = Guid.NewGuid();

        /// <summary>
        /// Gets inner data table
        /// IObservableDataTable interface implementation
        /// </summary>
        public IDataTable InnerDataTable
        {
            get
            {
                return _innerDataTable;
            }
        }

        /// <summary>
        /// Gets rows
        /// IObservableDataTable interface implementation
        /// </summary>
        public IList<IObservableDataRow> Rows
        {
            get
            {
                return _observableDataRows;
            }
        }

        /// <summary>
        /// Gets table name
        /// IObservableDataTable interface implementation
        /// </summary>
        public string TableName
        {
            get
            {
                return _innerDataTable.TableName;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Creates observable rows
        /// </summary>
        void CreateObservableRows()
        {
            for (int i = 0; i < _innerDataTable.Rows.Count; i++)
            {
                IDataRow dataRow = _innerDataTable.Rows[i];
                IObservableDataRow observableDataRow = new ObservableDataRow(dataRow);
                observableDataRow.DataFieldValueChanged += ObservableDataRow_DataFieldValueChanged;
                observableDataRow.RowStateChanged += ObservableDataRow_RowStateChanged;
                _observableDataRows.Add(observableDataRow);

                RaiseRowAddedEvent(_observableDataRows.Count - 1, observableDataRow);
            }
        }

        /// <summary>
        /// Raises RowAdded event
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <param name="observableDataRow">Observable data row</param>
        void RaiseRowAddedEvent(int rowIndex, IObservableDataRow observableDataRow)
        {
            if (RowsAdded != null)
            {
                RowsAdded(this, new RowsChangedEventArgs(this.TableName, rowIndex, observableDataRow));
            }
        }

        /// <summary>
        /// Raises row removed event
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <param name="observableDataRow">Observable data row</param>
        void RaiseRowRemovedEvent(int rowIndex, IObservableDataRow observableDataRow)
        {
            if (RowsRemoved != null)
            {
                RowsRemoved(this, new RowsChangedEventArgs(this.TableName, rowIndex, observableDataRow));
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles ObservableDataRow.DataFieldValueChanged event
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        void ObservableDataRow_DataFieldValueChanged(object? sender, DataFieldValueChangedEventArgs e)
        {
            if (DataFieldValueChanged != null)
            {
                DataFieldValueChanged(sender, e);
            }
        }

        /// <summary>
        /// Handles ObservableDataRow_RowStateChanged event
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void ObservableDataRow_RowStateChanged(object? sender, RowStateChangedEventArgs e)
        {
            if (RowStateChanged != null)
            {
                RowStateChanged(sender, e);
            }
        }
        #endregion
    }
}
