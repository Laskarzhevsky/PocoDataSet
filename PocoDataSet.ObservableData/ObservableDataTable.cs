using System;
using System.Collections.Generic;

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
        /// IObservableDataTable inteerface implementation
        /// </summary>
        public event EventHandler<DataFieldValueChangedEventArgs>? DataFieldValueChanged;

        /// <summary>
        /// RowsAdded event
        /// IObservableDataTable inteerface implementation
        /// </summary>
        public event EventHandler<RowsChangedEventArgs>? RowsAdded;

        /// <summary>
        /// RowsRemoved event
        /// IObservableDataTable inteerface implementation
        /// </summary>
        public event EventHandler<RowsChangedEventArgs>? RowsRemoved;
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
        /// Adds row
        /// IObservableDataTable inteerface implementation
        /// </summary>
        /// <param name="dataRow">Data row</param>
        public void AddRow(IDataRow dataRow)
        {
            IObservableDataRow observableDataRow = new ObservableDataRow(dataRow);
            observableDataRow.DataFieldValueChanged += ObservableDataRow_DataFieldValueChanged;
            _observableDataRows.Add(observableDataRow);
            RaiseRowAddedEvent(_observableDataRows.Count - 1, observableDataRow);
        }

        public void AddRow(IDataRow dataRow, IMergeManager? mergeManager)
        {
            IObservableDataRow observableDataRow = new ObservableDataRow(dataRow);
            if (mergeManager != null)
            {
                mergeManager.HandleMerge(TableName, observableDataRow);
            }

            observableDataRow.DataFieldValueChanged += ObservableDataRow_DataFieldValueChanged;
            _observableDataRows.Add(observableDataRow);
            RaiseRowAddedEvent(_observableDataRows.Count - 1, observableDataRow);
        }

        /// <summary>
        /// Removes row
        /// IObservableDataTable inteerface implementation
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        public void RemoveRow(int rowIndex)
        {
            IObservableDataRow observableDataRow = _observableDataRows[rowIndex];
            observableDataRow.DataFieldValueChanged -= ObservableDataRow_DataFieldValueChanged;
            _observableDataRows.Remove(observableDataRow);
            RaiseRowRemovedEvent(rowIndex);
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets list of columns
        /// IObservableDataTable inteerface implementation
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
        /// IObservableDataTable inteerface implementation
        /// </summary>
        public Guid Guid
        {
            get; set;
        } = Guid.NewGuid();

        /// <summary>
        /// Gets inner data table
        /// IObservableDataTable inteerface implementation
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
        /// IObservableDataTable inteerface implementation
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
        /// IObservableDataTable inteerface implementation
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
                AddRow(_innerDataTable.Rows[i]);
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
                RowsAdded(this, new RowsChangedEventArgs(rowIndex, observableDataRow));
            }
        }

        /// <summary>
        /// Raises row removed event
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        void RaiseRowRemovedEvent(int rowIndex)
        {
            if (RowsRemoved != null)
            {
                RowsRemoved(this, new RowsChangedEventArgs(rowIndex, null));
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
        #endregion
    }
}
