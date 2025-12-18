using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;
using PocoDataSet.IDataMerge;

namespace PocoDataSet.DataMerge
{
    /// <summary>
    /// Holds merge configuration for data set merge engine.
    /// </summary>
    public class DataSetMergeConfiguration : IDataSetMergeConfiguration
    {
        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DataSetMergeConfiguration()
        {
            ExcludeTablesFromMerge = new HashSet<string>();
            ExcludeTablesFromRowDeletion = new HashSet<string>();
            OverriddenPrimaryKeyNames = new Dictionary<string, List<string>>();
            TableHandlersByName = new Dictionary<string, ITableMergeHandler>();
            RowHandlersByTableName = new Dictionary<string, IRowMergeHandler>();
            DefaultValueProvider = new MetadataDefaultsProvider();
            DefaultTableMergeHandler = new DefaultTableMergeHandler();
            DefaultRowMergeHandler = new DefaultRowMergeHandler();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets data type default value provider 
        /// </summary>
        public IDataTypeDefaultValueProvider DefaultValueProvider
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets default row merge handler.
        /// IDataSetMergeConfiguration interface implementation
        /// </summary>
        public IRowMergeHandler DefaultRowMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets default table merge handler.
        /// IDataSetMergeConfiguration interface implementation
        /// </summary>
        public ITableMergeHandler DefaultTableMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets set of table names to exclude from merge.
        /// IDataSetMergeConfiguration interface implementation
        /// </summary>
        public ISet<string> ExcludeTablesFromMerge
        {
            get; private set;
        }

        /// <summary>
        /// Gets set of table names to exclude from row deletion.
        /// When a row exists in current table but not in refreshed table, it is not deleted for these tables.
        /// IDataSetMergeConfiguration interface implementation
        /// </summary>
        public ISet<string> ExcludeTablesFromRowDeletion
        {
            get; private set;
        }

        /// <summary>
        /// Gets dictionary of overridden primary key column names per table.
        /// IDataSetMergeConfiguration interface implementation
        /// </summary>
        public IDictionary<string, List<string>> OverriddenPrimaryKeyNames
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets flag indicating whether all rows in table will be replaced by rows from refreshed table
        /// IDataSetMergeConfiguration interface implementation
        /// if current table has no primary key defined.
        /// </summary>
        public bool ReplaceAllRowsInTableWhenTableHasNoPrimaryKeyDefined
        {
            get; set;
        }

        /// <summary>
        /// Gets row-specific handlers by table name.
        /// If not present, <see cref="DefaultRowMergeHandler"/> is used.
        /// IDataSetMergeConfiguration interface implementation
        /// </summary>
        public IDictionary<string, IRowMergeHandler> RowHandlersByTableName
        {
            get; private set;
        }

        /// <summary>
        /// Gets table-specific handlers by table name.
        /// IDataSetMergeConfiguration interface implementation
        /// </summary>
        public IDictionary<string, ITableMergeHandler> TableHandlersByName
        {
            get; private set;
        }
        #endregion
    }
}
