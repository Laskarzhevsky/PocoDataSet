using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Defines observable merge options functionality.
    /// </summary>
    public interface IObservableMergeOptions
    {
        #region Properties
        /// <summary>
        /// Gets data type default value provider
        /// </summary>
        IDataTypeDefaultValueProvider DataTypeDefaultValueProvider
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
        /// Gets or sets the mode used to merge data in the operation.
        /// </summary>
        /// <remarks>The merge mode determines how data is combined during the merge process. Different
        /// modes may affect the outcome of the merge, such as whether to overwrite existing data or to append new
        /// data.</remarks>
        MergeMode MergeMode
        {
            get; set;
        }

        /// <summary>
        /// Gets observable data set merge result
        /// </summary>
        IObservableDataSetMergeResult ObservableDataSetMergeResult
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
