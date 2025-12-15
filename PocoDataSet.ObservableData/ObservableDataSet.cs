using System;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableData
{
    /// <summary>
    /// Provides observable data set fuctionality
    /// </summary>
    public class ObservableDataSet : IObservableDataSet
    {
        #region Events
        /// <summary>
        /// Cell changed notification
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
        #endregion

        #region Data Fields
        /// <summary>
        /// Holds reference to inner IDataSet
        /// </summary>
        readonly IDataSet _innerDataSet;

        /// <summary>
        /// Holds reference to observable tables
        /// </summary>
        readonly IDictionary<string, IObservableDataTable> _observableDataTables = new Dictionary<string, IObservableDataTable>(StringComparer.Ordinal);

        /// <summary>
        /// Holds reference to observable tables
        /// </summary>
        readonly IDictionary<string, IObservableDataView> _observableDataViews = new Dictionary<string, IObservableDataView>(StringComparer.Ordinal);
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ObservableDataSet()
        {
            _innerDataSet = DataSetFactory.CreateDataSet();
            CreateObservableTables();
        }

        /// <summary>
        /// Creates instance of observable data set by taking POCO data set as an argument
        /// </summary>
        /// <param name="dataSet">Data set</param>
        public ObservableDataSet(IDataSet dataSet)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException(nameof(dataSet));
            }

            _innerDataSet = dataSet;
            CreateObservableTables();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets observable data view
        /// IObservableDataSet interface implementation
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="rowFilterString">Row filter string</param>
        /// <param name="caseSensitiveRowFilter">Flag indicating whether row filter is case sensitive</param>
        /// <param name="sortString">Sort string</param>
        /// <param name="requestorName">Requestor name</param>
        /// <returns>Observable data table</returns>
        public IObservableDataView? GetObservableDataView(string tableName, string? rowFilterString, bool caseSensitiveRowFilter, string? sortString, string? requestorName)
        {
            if (!string.IsNullOrEmpty(requestorName))
            {
                if (_observableDataViews.ContainsKey(tableName + requestorName))
                {
                    return _observableDataViews[tableName + requestorName];
                }
            }

            if (Tables.ContainsKey(tableName))
            {
                IObservableDataView? observableDataView = new ObservableDataView(Tables[tableName], rowFilterString, caseSensitiveRowFilter, sortString, requestorName);
                if (!string.IsNullOrEmpty(requestorName))
                {
                    _observableDataViews.Add(observableDataView.ViewName + requestorName, observableDataView);
                }

                return observableDataView;
            }

            return null;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets GUID
        /// IObservableDataSet interface implementation
        /// </summary>
        public Guid Guid
        {
            get; private set;
        } = Guid.NewGuid();

        /// <summary>
        /// Gets inner data set
        /// IObservableDataSet interface implementation
        /// </summary>
        public IDataSet InnerDataSet
        {
            get
            {
                return _innerDataSet;
            }
        }

        /// <summary>
        /// Gets relations
        /// IObservableDataSet interface implementation
        /// </summary>
        public List<IDataRelation> Relations
        {
            get
            {
                return _innerDataSet.Relations;
            }
        }

        /// <summary>
        /// Gets tables
        /// IObservableDataSet interface implementation
        /// </summary>
        public IDictionary<string, IObservableDataTable> Tables
        {
            get
            {
                return _observableDataTables;
            }
        }

        /// <summary>
        /// Gets views
        /// IObservableDataSet interface implementation
        /// </summary>
        public IDictionary<string, IObservableDataView> Views
        {
            get
            {
                return _observableDataViews;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Creates observable tables
        /// </summary>
        void CreateObservableTables()
        {
            foreach (IDataTable dataTable in _innerDataSet.Tables.Values)
            {
                IObservableDataTable? observableDataTable = new ObservableDataTable(dataTable);
                observableDataTable.DataFieldValueChanged += ObservableDataTable_DataFieldValueChanged;
                observableDataTable.RowsAdded += ObservableDataTable_RowsAdded;
                observableDataTable.RowsRemoved += ObservableDataTable_RowsRemoved;
                _observableDataTables.Add(observableDataTable.TableName, observableDataTable);
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles ObservableDataTable.DataFieldValueChanged event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        void ObservableDataTable_DataFieldValueChanged(object? sender, DataFieldValueChangedEventArgs e)
        {
            if (DataFieldValueChanged != null)
            {
                DataFieldValueChanged(sender, e);
            }
        }

        /// <summary>
        /// Handles ObservableDataTable.RowsAdded event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        void ObservableDataTable_RowsAdded(object? sender, RowsChangedEventArgs e)
        {
            if (RowsAdded != null)
            {
                RowsAdded(sender, e);
            }
        }

        /// <summary>
        /// Handles ObservableDataTable.RowsRemoved event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        void ObservableDataTable_RowsRemoved(object? sender, RowsChangedEventArgs e)
        {
            if (RowsRemoved != null)
            {
                RowsRemoved(sender, e);
            }
        }
        #endregion
    }
}
