using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableData
{
    /// <summary>
    /// Provides observable data viewS fuctionality
    /// </summary>
    public class ObservableDataView : IObservableDataView
    {
        #region Events
        /// <summary>
        /// Cell changed notification
        /// IObservableDataView interface implementation
        /// </summary>
        public event EventHandler<DataFieldValueChangedEventArgs>? DataFieldValueChanged;

        /// <summary>
        /// RowsAdded event
        /// IObservableDataView interface implementation
        /// </summary>
        public event EventHandler<RowsChangedEventArgs>? RowsAdded;

        /// <summary>
        /// RowsRemoved event
        /// IObservableDataView interface implementation
        /// </summary>
        public event EventHandler<RowsChangedEventArgs>? RowsRemoved;
        #endregion

        #region Data Fields
        /// <summary>
        /// Holds reference to inner observable data table
        /// </summary>
        readonly IObservableDataTable _innerObservableDataTable;

        /// <summary>
        /// Holds reference to observable data rows
        /// </summary>
        readonly List<IObservableDataRow> _observableDataRows = new List<IObservableDataRow>();

        /// <summary>
        /// Holds reference to rows selector
        /// </summary>
        Func<IDataRow, bool>? _rowsSelector;

        /// <summary>
        /// Holds reference to rows sorter
        /// </summary>
        IComparer<IObservableDataRow>? _rowsSorter;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="rowFilterString">Row filter string</param>
        /// <param name="caseSensitiveRowFilter">Flag indicating whether row filter is case sensitive</param>
        /// <param name="sortString">Sort string</param>
        /// <param name="requestorName">Requestor name</param>
        public ObservableDataView(IObservableDataTable observableDataTable, string? rowFilterString, bool caseSensitiveRowFilter, string? sortString, string? requestorName)
        {
            _innerObservableDataTable = observableDataTable;
            _innerObservableDataTable.DataFieldValueChanged += _innerObservableDataTable_DataFieldValueChanged;
            _innerObservableDataTable.RowsAdded += _innerObservableDataTable_RowsAdded;
            _innerObservableDataTable.RowsRemoved += _innerObservableDataTable_RowsRemoved;
            RequestorName = requestorName;
            SetRowFilter(rowFilterString, caseSensitiveRowFilter);
            SetSort(sortString);
            GetObservableRows();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds data row
        /// IObservableDataView interface implementation
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        public void AddRow(IObservableDataRow observableDataRow)
        {
            if (_rowsSelector == null)
            {
                observableDataRow.DataFieldValueChanged += ObservableDataRow_DataFieldValueChanged;
                _observableDataRows.Add(observableDataRow);
                RaiseRowAddedEvent(_observableDataRows.Count - 1, observableDataRow);
            }
            else
            {
                bool rowSelected = _rowsSelector(observableDataRow.InnerDataRow);
                if (rowSelected)
                {
                    observableDataRow.DataFieldValueChanged += ObservableDataRow_DataFieldValueChanged;
                    _observableDataRows.Add(observableDataRow);
                    RaiseRowAddedEvent(_observableDataRows.Count - 1, observableDataRow);
                }
            }
        }

        /// <summary>
        /// Removes row at specified index
        /// IObservableDataView interface implementation
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
        /// Gets columns
        /// IObservableDataView interface implementation
        /// </summary>
        public IList<IColumnMetadata> Columns
        {
            get
            {
                return _innerObservableDataTable.Columns;
            }
        }

        /// <summary>
        /// Gets GUID
        /// IObservableDataView interface implementation
        /// </summary>
        public Guid Guid
        {
            get; set;
        } = Guid.NewGuid();

        /// <summary>
        /// Gets inner observable data table
        /// IObservableDataView interface implementation
        /// </summary>
        public IObservableDataTable InnerObservableDataTable
        {
            get
            {
                return _innerObservableDataTable;
            }
        }

        /// <summary>
        /// Gets requestor name
        /// IObservableDataView interface implementation
        /// </summary>
        public string? RequestorName
        {
            get; set;
        }

        /// <summary>
        /// Gets rows
        /// IObservableDataView interface implementation
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
        /// IObservableDataView interface implementation
        /// </summary>
        public string ViewName
        {
            get
            {
                return _innerObservableDataTable.TableName;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets observable rows
        /// </summary>
        void GetObservableRows()
        {
            List<IObservableDataRow> selectedRows = new List<IObservableDataRow>();
            for (int i = 0; i < _innerObservableDataTable.Rows.Count; i++)
            {
                IObservableDataRow observableDataRow = _innerObservableDataTable.Rows[i];
                if (_rowsSelector == null)
                {
                    selectedRows.Add(observableDataRow);
                }
                else
                {
                    bool rowSelected = _rowsSelector(observableDataRow.InnerDataRow);
                    if (rowSelected)
                    {
                        selectedRows.Add(observableDataRow);
                    }
                }
            }

            if (_rowsSorter != null)
            {
                selectedRows.Sort(_rowsSorter);
            }

            for (int i = 0; i < selectedRows.Count; i++)
            {
                IObservableDataRow observableDataRow = selectedRows[i];
                _observableDataRows.Add(observableDataRow);
                RaiseRowAddedEvent(i, observableDataRow);
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
        /// Raises RowRemoved event
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        void RaiseRowRemovedEvent(int rowIndex)
        {
            if (RowsRemoved != null)
            {
                RowsRemoved(this, new RowsChangedEventArgs(rowIndex, null));
            }
        }

        /// <summary>
        /// Sets row filter
        /// </summary>
        /// <param name="rowFilterString">Row filter string</param>
        /// <param name="caseSensitiveRowFilter">Flag indicating whether row filter is case sensitive</param>
        void SetRowFilter(string? rowFilterString, bool caseSensitiveRowFilter)
        {
            if (string.IsNullOrEmpty(rowFilterString))
            {
                return;
            }

            IRowFilter rowFilter = RowFilterCompiler.Build(rowFilterString, caseSensitiveRowFilter);
            _rowsSelector = new Func<IDataRow, bool>(rowFilter.Include);
        }

        /// <summary>
        /// Sets sort
        /// </summary>
        /// <param name="sortString">Sort string</param>
        void SetSort(string? sortString)
        {
            if (string.IsNullOrEmpty(sortString))
            {
                return;
            }

            IComparer <IObservableDataRow> dataRowComparer = SortCompiler.Build(sortString);
            _rowsSorter = dataRowComparer;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles ObservableDataRow.DataFieldValueChanged evnet
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        void ObservableDataRow_DataFieldValueChanged(object? sender, DataFieldValueChangedEventArgs e)
        {
            if (DataFieldValueChanged != null)
            {
                DataFieldValueChanged(sender, e);
            }
        }


        void _innerObservableDataTable_RowsRemoved(object? sender, RowsChangedEventArgs e)
        {
        }

        void _innerObservableDataTable_RowsAdded(object? sender, RowsChangedEventArgs e)
        {
            if (e.ObservableDataRow != null)
            {
                AddRow(e.ObservableDataRow);
            }
        }

        void _innerObservableDataTable_DataFieldValueChanged(object? sender, DataFieldValueChangedEventArgs e)
        {
        }
        #endregion
    }
}
