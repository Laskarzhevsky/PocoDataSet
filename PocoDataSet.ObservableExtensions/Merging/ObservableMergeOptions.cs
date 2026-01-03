using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Provides observable merge options functionality
    /// </summary>
    public class ObservableMergeOptions : IObservableMergeOptions
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ObservableMergeOptions()
        {
            ObservableDataSetMergeHandlers = new Dictionary<string, IObservableDataSetMergeHandler>();
            ObservableDataSetMergeResult = new ObservableDataSetMergeResult(new List<IObservableDataSetMergeResultEntry>(), new List<IObservableDataSetMergeResultEntry>(), new List<IObservableDataSetMergeResultEntry>());
            DefaultObservableDataSetMergeHandler = new ObservableDataSetDefaultMergeHandler();
            DefaultObservableRowMergeHandler = new ObservableDataRowDefaultMergeHandler();
            DefaultObservableTableMergeHandler = new ObservableDataTableDefaultMergeHandler();
            DataTypeDefaultValueProvider = new MetadataDefaultsProvider();
            ObservableRowMergeHandlers = new Dictionary<string, IObservableDataRowMergeHandler>();
            ObservableTableMergeHandlers = new Dictionary<string, IObservableDataTableMergeHandler>();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets data type default value provider 
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public IDataTypeDefaultValueProvider DataTypeDefaultValueProvider
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets default observable data set merge handler
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public IObservableDataSetMergeHandler DefaultObservableDataSetMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets default observable row merge handler
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public IObservableDataRowMergeHandler DefaultObservableRowMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets default observable table merge handler
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public IObservableDataTableMergeHandler DefaultObservableTableMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets list of table names which need to be excluded from merge
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public List<string> ExcludeTablesFromMerge
        {
            get;
        } = new List<string>();

        /// <summary>
        /// Gets list of table names which rows need to be excluded from deletion during the merge process
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public List<string> ExcludeTablesFromRowDeletion
        {
            get;
        } = new List<string>();

        /// <summary>
        /// Gets merge mode
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public MergeMode MergeMode
        {
            get; set;
        } = MergeMode.Refresh;

        /// <summary>
        /// Gets observable data set merge handlers
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public IDictionary<string, IObservableDataSetMergeHandler> ObservableDataSetMergeHandlers
        {
            get; private set;
        }

        /// <summary>
        /// Gets observable data set merge result
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public IObservableDataSetMergeResult ObservableDataSetMergeResult
        {
            get; private set;
        }

        /// <summary>
        /// Gets overridden primary key names to replace primary keys defined by table schema
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public IDictionary<string, List<string>> OverriddenPrimaryKeyNames
        {
            get;
        } = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        /// <summary>
        /// Gets observable row merge handlers
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public IDictionary<string, IObservableDataRowMergeHandler> ObservableRowMergeHandlers
        {
            get; private set;
        }

        /// <summary>
        /// Gets observable table merge handlers
        /// IObservableMergeOptions interface implementation
        /// </summary>
        public IDictionary<string, IObservableDataTableMergeHandler> ObservableTableMergeHandlers
        {
            get; private set;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets observable data set merge handler
        /// IObservableMergeOptions interface implementation
        /// </summary>
        /// <param name="dataSetMergeHandlerKey">Data set merge handler key</param>
        /// <returns>Data set merge handler</returns>
        public IObservableDataSetMergeHandler GetObservableDataSetMergeHandler(string? dataSetMergeHandlerKey)
        {
            IObservableDataSetMergeHandler? observableDataSetMergeHandler;
            if (!string.IsNullOrEmpty(dataSetMergeHandlerKey) && ObservableDataSetMergeHandlers.TryGetValue(dataSetMergeHandlerKey, out observableDataSetMergeHandler))
            {
                return observableDataSetMergeHandler;
            }

            return DefaultObservableDataSetMergeHandler;
        }

        /// <summary>
        /// Gets primary key column names for a given table, applying overrides when configured.
        /// IObservableMergeOptions interface implementation
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>Primary key column names</returns>
        public List<string> GetPrimaryKeyColumnNames(IObservableDataTable dataTable)
        {
            List<string>? overrideKeys;
            if (OverriddenPrimaryKeyNames.TryGetValue(dataTable.TableName, out overrideKeys))
            {
                if (overrideKeys != null && overrideKeys.Count > 0)
                {
                    return new List<string>(overrideKeys);
                }
            }

            return dataTable.InnerDataTable.GetPrimaryKeyColumnNames(null);
        }

        /// <summary>
        /// Gets observable row merge handler
        /// IObservableMergeOptions interface implementation
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Row merge handler</returns>
        public IObservableDataRowMergeHandler GetObservableRowMergeHandler(string tableName)
        {
            IObservableDataRowMergeHandler? observableRowMergeHandler;
            if (ObservableRowMergeHandlers.TryGetValue(tableName, out observableRowMergeHandler))
            {
                return observableRowMergeHandler;
            }

            return DefaultObservableRowMergeHandler;
        }

        /// <summary>
        /// Gets observable table merge handler
        /// IObservableMergeOptions interface implementation
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Table merge handler</returns>
        public IObservableDataTableMergeHandler GetObservableTableMergeHandler(string tableName)
        {
            IObservableDataTableMergeHandler? observableTableMergeHandler;
            if (ObservableTableMergeHandlers.TryGetValue(tableName, out observableTableMergeHandler))
            {
                return observableTableMergeHandler;
            }

            return DefaultObservableTableMergeHandler;
        }
        #endregion
    }
}
