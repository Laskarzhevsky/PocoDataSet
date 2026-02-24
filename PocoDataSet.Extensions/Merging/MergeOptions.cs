using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides merge options functionality.
    /// Note: merge options are intentionally policy-free; each merge type has its own dedicated implementation chain.
    /// </summary>
    public class MergeOptions : IMergeOptions
    {
        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MergeOptions()
        {
            DataSetMergeResult = new DataSetMergeResult(
                new List<IDataSetMergeResultEntry>(),
                new List<IDataSetMergeResultEntry>(),
                new List<IDataSetMergeResultEntry>());

            ExcludeTablesFromMerge = new List<string>();
            ExcludeTablesFromRowDeletion = new List<string>();
            OverriddenPrimaryKeyNames = new Dictionary<string, List<string>>();
        }
        #endregion

        #region Public Properties
        public IDataSetMergeResult DataSetMergeResult { get; private set; }

        public List<string> ExcludeTablesFromMerge { get; private set; }

        public List<string> ExcludeTablesFromRowDeletion { get; private set; }

        public MergeMode MergeMode
        {
            get; set;
        }

        public IDictionary<string, List<string>> OverriddenPrimaryKeyNames { get; private set; }
        #endregion

        #region Public Methods
        public List<string> GetPrimaryKeyColumnNames(IDataTable dataTable)
        {
            if (OverriddenPrimaryKeyNames.TryGetValue(dataTable.TableName, out List<string>? overridden) &&
                overridden != null &&
                overridden.Count > 0)
            {
                return overridden;
            }

            // IDataTable.PrimaryKeys is authoritative.
            return new List<string>(dataTable.PrimaryKeys);
        }
        #endregion
    }
}
