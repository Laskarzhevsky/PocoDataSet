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
        /// Gets data set merge result
        /// </summary>
        IDataSetMergeResult DataSetMergeResult
        {
            get; 
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
        #endregion

        #region Methods
        /// <summary>
        /// Gets primary key column names for a given table, applying overrides when configured.
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>Primary key column names</returns>
        List<string> GetPrimaryKeyColumnNames(IDataTable dataTable);
        #endregion
    }
}
