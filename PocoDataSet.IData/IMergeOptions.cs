using System;
using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Holds merge options
    /// </summary>
    public interface IMergeOptions
    {
        #region Properties
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
        /// Gets or sets GetPruneFilter
        /// </summary>
        public Func<string, Func<IDataRow, bool>?> GetPruneFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets overridden primary key names to replace primary keys defined by table schema
        /// </summary>
        IDictionary<string, List<string>> OverriddenPrimaryKeyNames
        {
            get;
        }

        /// <summary>
        /// Gets names of tables which need to be deleted from the current data set if tables with these names in the refreshed data set have no rows
        /// </summary>
        ISet<string> PruneTables
        {
            get;
        }

        /// <summary>
        /// Gets or sets flag indicating whether all rows in table needs to be replaced by rows from refreshed table if current table has no primary key defined
        /// </summary>
        public bool ReplaceAllRowsInTableWhenTableHasNoPrimaryKeyDefined
        {
            get; set;
        }
        #endregion
    }
}
