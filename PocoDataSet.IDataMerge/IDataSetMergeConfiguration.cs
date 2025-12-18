using System.Collections.Generic;

namespace PocoDataSet.IDataMerge
{
    /// <summary>
    /// Defines data set merge configuration functionality
    /// </summary>
    public interface IDataSetMergeConfiguration
    {
        #region Properties
        /// <summary>
        /// Gets or sets default row merge handler.
        /// </summary>
        IRowMergeHandler DefaultRowMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets default table merge handler.
        /// </summary>
        ITableMergeHandler DefaultTableMergeHandler
        {
            get; set;
        }

        /// <summary>
        /// Gets set of table names to exclude from merge.
        /// </summary>
        ISet<string> ExcludeTablesFromMerge
        {
            get;
        }

        /// <summary>
        /// Gets set of table names to exclude from row deletion.
        /// When a row exists in current table but not in refreshed table, it is not deleted for these tables.
        /// </summary>
        ISet<string> ExcludeTablesFromRowDeletion
        {
            get;
        }

        /// <summary>
        /// Gets dictionary of overridden primary key column names per table.
        /// </summary>
        IDictionary<string, List<string>> OverriddenPrimaryKeyNames
        {
            get;
        }

        /// <summary>
        /// Gets or sets flag indicating whether all rows in table will be replaced by rows from refreshed table
        /// if current table has no primary key defined.
        /// </summary>
        bool ReplaceAllRowsInTableWhenTableHasNoPrimaryKeyDefined
        {
            get; set;
        }

        /// <summary>
        /// Gets row-specific handlers by table name.
        /// If not present, <see cref="DefaultRowMergeHandler"/> is used.
        /// </summary>
        IDictionary<string, IRowMergeHandler> RowHandlersByTableName
        {
            get;
        }

        /// <summary>
        /// Gets table-specific handlers by table name.
        /// </summary>
        IDictionary<string, ITableMergeHandler> TableHandlersByName
        {
            get;
        }
        #endregion
    }
}
