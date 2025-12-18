using System.Collections.Generic;
using PocoDataSet.IData;

namespace PocoDataSet.IDataMerge
{
    /// <summary>
    /// Provides merge context for a single merge operation.
    /// </summary>
    public interface IMergeContext
    {
        #region Properties
        /// <summary>
        /// Gets merge configuration.
        /// </summary>
        IDataSetMergeConfiguration Configuration
        {
            get;
        }

        /// <summary>
        /// Gets merge result.
        /// </summary>
        public IDataSetsMergeResult Result
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
