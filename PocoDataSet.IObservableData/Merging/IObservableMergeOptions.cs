using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Holds merge options
    /// </summary>
    public interface IObservableMergeOptions
    {
        #region Properties
        /// <summary>
        /// Gets observable data set merge handlers
        /// </summary>
        IDictionary<string, IObservableDataSetMergeHandler> ObservableDataSetMergeHandlers
        {
            get;
        }

        /// <summary>
        /// Gets observable data set merge result
        /// </summary>
        IObservableDataSetMergeResult ObservableDataSetMergeResult
        {
            get;
        }

        /// <summary>
        /// Gets or sets default observable data set merge handler
        /// </summary>
        IObservableDataSetMergeHandler DefaultObservableDataSetMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets default observable row merge handler
        /// </summary>
        IObservableDataRowMergeHandler DefaultObservableRowMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets default observable table merge handler
        /// </summary>
        IObservableDataTableMergeHandler DefaultObservableTableMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets data type default value provider 
        /// </summary>
        public IDataTypeDefaultValueProvider DataTypeDefaultValueProvider
        {
            get;
        }

        /// <summary>
        /// Gets list of table names which need to be excluded from merge
        /// Data in mentioned tables will not be changed during the merge process
        /// </summary>
        List<string> ExcludeTablesFromMerge
        {
            get;
        }

        /// <summary>
        /// Gets list of table names which rows need to be excluded from deletion during the merge process
        /// </summary>
        List<string> ExcludeTablesFromRowDeletion
        {
            get;
        }

        /// <summary>
        /// Gets overridden primary key names to replace primary keys defined by table schema
        /// </summary>
        IDictionary<string, List<string>> OverriddenPrimaryKeyNames
        {
            get;
        }

        /// <summary>
        /// Gets observable row merge handlers
        /// </summary>
        IDictionary<string, IObservableDataRowMergeHandler> ObservableRowMergeHandlers
        {
            get;
        }

        /// <summary>
        /// Gets observable table merge handlers
        /// </summary>
        IDictionary<string, IObservableDataTableMergeHandler> ObservableTableMergeHandlers
        {
            get;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets observable data set merge handler
        /// </summary>
        /// <param name="dataSetMergeHandlerKey">Data set merge handler key</param>
        /// <returns>Data set merge handler</returns>
        IObservableDataSetMergeHandler GetObservableDataSetMergeHandler(string? dataSetMergeHandlerKey);

        /// <summary>
        /// Gets primary key column names for a given table, applying overrides when configured.
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>Primary key column names</returns>
        List<string> GetPrimaryKeyColumnNames(IObservableDataTable dataTable);

        /// <summary>
        /// Gets observable row merge handler
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Row merge handler</returns>
        IObservableDataRowMergeHandler GetObservableRowMergeHandler(string tableName);

        /// <summary>
        /// Gets observable table merge handler
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Table merge handler</returns>
        IObservableDataTableMergeHandler GetObservableTableMergeHandler(string tableName);
        #endregion
    }
}
