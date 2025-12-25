using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides merge options functionality
    /// </summary>
    public class MergeOptions : IMergeOptions
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public MergeOptions()
        {
            DataSetMergeHandlers = new Dictionary<string, IDataSetMergeHandler>();
            DataSetMergeResult = new DataSetMergeResult(new List<IDataSetMergeResultEntry>(), new List<IDataSetMergeResultEntry>(), new List<IDataSetMergeResultEntry>());
            DefaultDataSetMergeHandler = new DataSetDefaultMergeHandler();
            DefaultRowMergeHandler = new DataRowDefaultMergeHandler();
            DefaultTableMergeHandler = new DataTableDefaultMergeHandler();
            DataTypeDefaultValueProvider = new MetadataDefaultsProvider();
            RowMergeHandlers = new Dictionary<string, IRowMergeHandler>();
            TableMergeHandlers = new Dictionary<string, ITableMergeHandler>();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets data set merge handlers
        /// IMergeOptions interface implementation
        /// </summary>
        public IDictionary<string, IDataSetMergeHandler> DataSetMergeHandlers
        {
            get; private set;
        }

        /// <summary>
        /// Gets data set merge result
        /// IMergeOptions interface implementation
        /// </summary>
        public IDataSetMergeResult DataSetMergeResult
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets default data set merge handler
        /// IMergeOptions interface implementation
        /// </summary>
        public IDataSetMergeHandler DefaultDataSetMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets default row merge handler
        /// IMergeOptions interface implementation
        /// </summary>
        public IRowMergeHandler DefaultRowMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets default table merge handler
        /// IMergeOptions interface implementation
        /// </summary>
        public ITableMergeHandler DefaultTableMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets data type default value provider 
        /// IMergeOptions interface implementation
        /// </summary>
        public IDataTypeDefaultValueProvider DataTypeDefaultValueProvider
        {
            get; private set;
        }

        /// <summary>
        /// Gets list of table names which need to be excluded from merge
        /// IMergeOptions interface implementation
        /// </summary>
        public List<string> ExcludeTablesFromMerge
        {
            get;
        } = new List<string>();

        /// <summary>
        /// Gets list of table names which rows need to be excluded from deletion during the merge process
        /// IMergeOptions interface implementation
        /// </summary>
        public List<string> ExcludeTablesFromRowDeletion
        {
            get;
        } = new List<string>();

        /// <summary>
        /// Gets overridden primary key names to replace primary keys defined by table schema
        /// IMergeOptions interface implementation
        /// </summary>
        public IDictionary<string, List<string>> OverriddenPrimaryKeyNames
        {
            get;
        } = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        /// <summary>
        /// Gets row merge handlers
        /// IMergeOptions interface implementation
        /// </summary>
        public IDictionary<string, IRowMergeHandler> RowMergeHandlers
        {
            get; private set;
        }

        /// <summary>
        /// Gets table merge handlers
        /// IMergeOptions interface implementation
        /// </summary>
        public IDictionary<string, ITableMergeHandler> TableMergeHandlers
        {
            get; private set;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets data set merge handler
        /// IMergeOptions interface implementation
        /// </summary>
        /// <param name="dataSetMergeHandlerKey">Data set merge handler key</param>
        /// <returns>Data set merge handler</returns>
        public IDataSetMergeHandler GetDataSetMergeHandler(string? dataSetMergeHandlerKey)
        {
            IDataSetMergeHandler? dataSetMergeHandler;
            if (!string.IsNullOrEmpty(dataSetMergeHandlerKey) && DataSetMergeHandlers.TryGetValue(dataSetMergeHandlerKey, out dataSetMergeHandler))
            {
                return dataSetMergeHandler;
            }

            return DefaultDataSetMergeHandler;
        }

        /// <summary>
        /// Gets primary key column names for a given table, applying overrides when configured.
        /// IMergeContext interface implementation
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>Primary key column names</returns>
        public List<string> GetPrimaryKeyColumnNames(IDataTable dataTable)
        {
            List<string>? overrideKeys;
            if (OverriddenPrimaryKeyNames.TryGetValue(dataTable.TableName, out overrideKeys))
            {
                if (overrideKeys != null && overrideKeys.Count > 0)
                {
                    return new List<string>(overrideKeys);
                }
            }

            return dataTable.GetPrimaryKeyColumnNames(null);
        }

        /// <summary>
        /// Gets row merge handler
        /// IMergeOptions interface implementation
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Row merge handler</returns>
        public IRowMergeHandler GetRowMergeHandler(string tableName)
        {
            IRowMergeHandler? rowMergeHandler;
            if (RowMergeHandlers.TryGetValue(tableName, out rowMergeHandler))
            {
                return rowMergeHandler;
            }

            return DefaultRowMergeHandler;
        }

        /// <summary>
        /// Gets table merge handler
        /// IMergeOptions interface implementation
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Table merge handler</returns>
        public ITableMergeHandler GetTableMergeHandler(string tableName)
        {
            ITableMergeHandler? tableMergeHandler;
            if (TableMergeHandlers.TryGetValue(tableName, out tableMergeHandler))
            {
                return tableMergeHandler;
            }

            return DefaultTableMergeHandler;
        }
        #endregion
    }
}
