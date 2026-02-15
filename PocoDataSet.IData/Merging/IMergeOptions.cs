using System;
using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines merge options functionality
    /// </summary>
    public interface IMergeOptions
    {
        #region Properties
        /// <summary>
        /// Gets data set merge handlers
        /// </summary>
        IDictionary<string, IDataSetMergeHandler> DataSetMergeHandlers
        {
            get;
        }

        /// <summary>
        /// Gets data set merge result
        /// </summary>
        IDataSetMergeResult DataSetMergeResult
        {
            get;
        }

        /// <summary>
        /// Gets or sets default data set merge handler
        /// </summary>
        IDataSetMergeHandler DefaultDataSetMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets default row merge handler
        /// </summary>
        IRowMergeHandler DefaultRowMergeHandler
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
        /// Gets or sets merge mode
        /// </summary>
        MergeMode MergeMode
        {
            get; set;
        }

        /// <summary>
        /// Gets overridden primary key names to replace primary keys defined by table schema
        /// </summary>
        IDictionary<string, List<string>> OverriddenPrimaryKeyNames
        {
            get;
        }

        /// <summary>
        /// Gets row merge handlers
        /// </summary>
        IDictionary<string, IRowMergeHandler> RowMergeHandlers
        {
            get;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets data set merge handler
        /// </summary>
        /// <param name="dataSetMergeHandlerKey">Data set merge handler key</param>
        /// <returns>Data set merge handler</returns>
        IDataSetMergeHandler GetDataSetMergeHandler(string? dataSetMergeHandlerKey);

        /// <summary>
        /// Gets primary key column names for a given table, applying overrides when configured.
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>Primary key column names</returns>
        List<string> GetPrimaryKeyColumnNames(IDataTable dataTable);

        /// <summary>
        /// Gets row merge handler
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Row merge handler</returns>
        IRowMergeHandler GetRowMergeHandler(string tableName);
        #endregion
    }
}
