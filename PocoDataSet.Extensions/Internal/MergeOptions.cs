using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Holds merge options
    /// </summary>
    public sealed class MergeOptions : IMergeOptions
    {
        #region Data Fields
        /// <summary>Optional per-table filter that limits pruning/upserts to a scope (e.g., Breadcrumbs == "Libraries").</summary>
        Func<string, Func<IDataRow, bool>?> _getPruneFilter;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public MergeOptions()
        {
            _getPruneFilter = new Func<string, Func<IDataRow, bool>?>(DefaultGetPruneFilter);
        }
        #endregion

        #region Public Properties
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
        /// </summary>
        public List<string> ExcludeTablesFromRowDeletion
        {
            get;
        } = new List<string>();

        /// <summary>
        /// Gets or sets GetPruneFilter
        /// IMergeOptions interface implementation
        /// </summary>
        public Func<string, Func<IDataRow, bool>?> GetPruneFilter
        {
            get
            {
                return _getPruneFilter;
            }
            set
            {
                if (value == null)
                {
                    _getPruneFilter = new Func<string, Func<IDataRow, bool>?>(DefaultGetPruneFilter);
                }
                else
                {
                    _getPruneFilter = value;
                }
            }
        }

        /// <summary>
        /// Gets overridden primary key names to replace primary keys defined by table schema
        /// IMergeOptions interface implementation
        /// </summary>
        public IDictionary<string, List<string>> OverriddenPrimaryKeyNames
        {
            get;
        } = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        /// <summary>
        /// Gets names of tables which need to be deleted from the current data set if tables with these names in the refreshed data set have no rows
        /// IMergeOptions interface implementation
        /// </summary>
        public ISet<string> PruneTables
        {
            get;
        } = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// Gets or sets flag indicating whether table needs to be replaced if it has no primary key defined
        /// IMergeOptions interface implementation
        /// </summary>
        public bool ReplaceAllRowsInTableWhenTableHasNoPrimaryKeyDefined
        {
            get; set;
        } = true;
        #endregion

        #region Methods
        /// <summary>
        /// Gets default GetPruneFilter
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Default GetPruneFilter</returns>
        static Func<IDataRow, bool>? DefaultGetPruneFilter(string tableName)
        {
            return null; // no scope by default
        }
        #endregion
    }
}
