using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        #region Data Fields
        /// <summary>
        /// Holds reference to observable tables
        /// </summary>
        readonly Dictionary<string, IObservableDataTable> _observableDataTables = new Dictionary<string, IObservableDataTable>(StringComparer.Ordinal);

        /// <summary>
        /// Provides a read-only view over observable tables to prevent external mutation
        /// </summary>
        readonly ReadOnlyDictionary<string, IObservableDataTable> _readOnlyTables;
        #endregion

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
        public event EventHandler<RowsChangedEventArgs>? RowAdded;

        /// <summary>
        /// RowsRemoved event
        /// IObservableDataTable interface implementation
        /// </summary>
        public event EventHandler<RowsChangedEventArgs>? RowRemoved;

        /// <summary>
        /// TableAdded event
        /// IObservableDataTable interface implementation
        /// </summary>
        public event EventHandler<TablesChangedEventArgs>? TableAdded;

        /// <summary>
        /// TableRemoved event
        /// IObservableDataTable interface implementation
        /// </summary>
        public event EventHandler<TablesChangedEventArgs>? TableRemoved;
        #endregion

        #region Data Fields
        /// <summary>
        /// Holds reference to inner IDataSet
        /// </summary>
        readonly IDataSet _innerDataSet;
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
            _readOnlyTables = new ReadOnlyDictionary<string, IObservableDataTable>(_observableDataTables);
            _innerDataSet = DataSetFactory.CreateDataSet();
            CreateObservableTables();
        }

        /// <summary>
        /// Creates instance of observable data set by taking POCO data set as an argument
        /// </summary>
        /// <param name="dataSet">Data set</param>
        public ObservableDataSet(IDataSet dataSet)
        {
            _readOnlyTables = new ReadOnlyDictionary<string, IObservableDataTable>(_observableDataTables);
            _innerDataSet = dataSet;
            CreateObservableTables();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds observable table
        /// IObservableDataSet interface implementation
        /// </summary>
        /// <param name="dataTable">Data table for addition</param>
        /// <returns>Added observable data table</returns>
        public IObservableDataTable AddObservableTable(IDataTable dataTable)
        {
            IObservableDataTable? observableDataTable = new ObservableDataTable(dataTable);
            observableDataTable.DataFieldValueChanged += ObservableDataTable_DataFieldValueChanged;
            observableDataTable.RowsAdded += ObservableDataTable_RowsAdded;
            observableDataTable.RowsRemoved += ObservableDataTable_RowsRemoved;
            _observableDataTables.Add(observableDataTable.TableName, observableDataTable);
            if (TableAdded != null)
            {
                TableAdded(this, new TablesChangedEventArgs(observableDataTable.TableName));
            }

            return observableDataTable;
        }

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
        public IObservableDataView? GetObservableDataView(string tableName, string? rowFilterString, bool caseSensitiveRowFilter, string? sortString, string requestorName)
        {
            string viewKey = BuildObservableDataViewKey(tableName, requestorName);
            if (_observableDataViews.ContainsKey(viewKey))
            {
                return _observableDataViews[viewKey];
            }

            if (_observableDataTables.ContainsKey(tableName))
            {
                IObservableDataView? observableDataView = new ObservableDataView(_observableDataTables[tableName], rowFilterString, caseSensitiveRowFilter, sortString, requestorName);
                _observableDataViews.Add(viewKey, observableDataView);

                return observableDataView;
            }

            return null;
        }

        /// <summary>
        /// Removes observable data view from cache and disposes it
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="requestorName">Requestor name</param>
        /// <returns>Flag indicating whether view was removed</returns>
        public bool RemoveObservableDataView(string tableName, string requestorName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(requestorName))
            {
                return false;
            }

            string viewKey = BuildObservableDataViewKey(tableName, requestorName);
            if (!_observableDataViews.ContainsKey(viewKey))
            {
                return false;
            }

            DisposeObservableDataViewByKey(viewKey);
            return true;
        }

        /// <summary>
        /// Removes all observable data views for the specified requestor and disposes them
        /// </summary>
        /// <param name="requestorName">Requestor name</param>
        /// <returns>Number of removed views</returns>
        public int RemoveObservableDataViewsForRequestor(string requestorName)
        {
            if (string.IsNullOrWhiteSpace(requestorName))
            {
                return 0;
            }

            List<string> keysToRemove = new List<string>();
            foreach (KeyValuePair<string, IObservableDataView> pair in _observableDataViews)
            {
                if (pair.Key.EndsWith("|" + requestorName, StringComparison.Ordinal))
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            for (int i = 0; i < keysToRemove.Count; i++)
            {
                DisposeObservableDataViewByKey(keysToRemove[i]);
            }

            return keysToRemove.Count;
        }


        /// <summary>
        /// Removes observable table
        /// </summary>
        /// <param name="tableName">Table name</param>
        public void RemoveObservableTable(string tableName)
        {
            if (_observableDataTables.ContainsKey(tableName))
            {
                IObservableDataTable observableDataTable = _observableDataTables[tableName];

                // Remove observable views that point at this table (they subscribe to table events)
                List<string> keysToRemove = new List<string>();
                foreach (KeyValuePair<string, IObservableDataView> pair in _observableDataViews)
                {
                    IObservableDataTable? innerTable = pair.Value.InnerObservableDataTable;
                    if (ReferenceEquals(pair.Value.InnerObservableDataTable, observableDataTable))
                    {
                        keysToRemove.Add(pair.Key);
                    }
                }

                for (int i = 0; i < keysToRemove.Count; i++)
                {
                    DisposeObservableDataViewByKey(keysToRemove[i]);
                }

                // Detach dataset event handlers from table
                observableDataTable.DataFieldValueChanged -= ObservableDataTable_DataFieldValueChanged;
                observableDataTable.RowsAdded -= ObservableDataTable_RowsAdded;
                observableDataTable.RowsRemoved -= ObservableDataTable_RowsRemoved;

                if (TableRemoved != null)
                {
                    TableRemoved(this, new TablesChangedEventArgs(tableName));
                }

                _observableDataTables.Remove(tableName);

                // Keep inner dataset consistent
                if (_innerDataSet.Tables.ContainsKey(tableName))
                {
                    _innerDataSet.Tables.Remove(tableName);
                }
            }
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
        /// Gets or sets name
        /// IObservableDataSet interface implementation
        /// </summary>
        public string? Name
        {
            get; set;
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
        public IReadOnlyDictionary<string, IObservableDataTable> Tables
        {
            get
            {
                return _readOnlyTables;
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
        /// Builds observable data view key
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="requestorName">Requestor name</param>
        /// <returns>View key</returns>
        static string BuildObservableDataViewKey(string tableName, string requestorName)
        {
            return tableName + "|" + requestorName;
        }

        /// <summary>
        /// Creates observable tables
        /// </summary>
        void CreateObservableTables()
        {
            foreach (IDataTable dataTable in _innerDataSet.Tables.Values)
            {
                AddObservableTable(dataTable);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Disposes observable data view by key
        /// </summary>
        /// <param name="viewKey">View key</param>
        private void DisposeObservableDataViewByKey(string viewKey)
        {
            if (string.IsNullOrWhiteSpace(viewKey))
            {
                return;
            }

            if (!_observableDataViews.ContainsKey(viewKey))
            {
                return;
            }

            IObservableDataView observableDataView = _observableDataViews[viewKey];

            // Remove first, then dispose (prevents re-entrancy issues if disposal triggers anything)
            _observableDataViews.Remove(viewKey);

            AsyncDisposableObject? asyncDisposableObject = observableDataView as AsyncDisposableObject;
            if (asyncDisposableObject != null)
            {
                asyncDisposableObject.Dispose();
                return;
            }

            IDisposable? disposable = observableDataView as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
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
            if (RowAdded != null)
            {
                RowAdded(sender, e);
            }
        }

        /// <summary>
        /// Handles ObservableDataTable.RowsRemoved event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        void ObservableDataTable_RowsRemoved(object? sender, RowsChangedEventArgs e)
        {
            if (RowRemoved != null)
            {
                RowRemoved(sender, e);
            }
        }
        #endregion
    }
}
