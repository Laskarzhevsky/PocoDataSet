using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IDataMerge;

namespace PocoDataSet.DataMerge
{
    /// <summary>
    /// Provides merge context for a single merge operation.
    /// </summary>
    public class MergeContext : IMergeContext
    {
        #region Constructors
        /// <summary>
        /// Creates context.
        /// </summary>
        /// <param name="configuration">Merge configuration.</param>
        /// <param name="result">Merge result.</param>
        public MergeContext(IDataSetMergeConfiguration configuration, IDataSetsMergeResult result)
        {
            Configuration = configuration;
            Result = result;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets merge configuration.
        /// IMergeContext interface implementation
        /// </summary>
        public IDataSetMergeConfiguration Configuration
        {
            get; private set;
        }

        /// <summary>
        /// Gets merge result.
        /// IMergeContext interface implementation
        /// </summary>
        public IDataSetsMergeResult Result
        {
            get; private set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets primary key column names for a given table, applying overrides when configured.
        /// IMergeContext interface implementation
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>Primary key column names</returns>
        public List<string> GetPrimaryKeyColumnNames(IDataTable dataTable)
        {
            List<string>? overrideKeys;
            if (Configuration.OverriddenPrimaryKeyNames.TryGetValue(dataTable.TableName, out overrideKeys))
            {
                if (overrideKeys != null && overrideKeys.Count > 0)
                {
                    return new List<string>(overrideKeys);
                }
            }

            return dataTable.GetPrimaryKeyColumnNames(null);
        }
        #endregion
    }
}
