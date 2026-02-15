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
            DataSetMergeResult = new DataSetMergeResult(
                new List<IDataSetMergeResultEntry>(),
                new List<IDataSetMergeResultEntry>(),
                new List<IDataSetMergeResultEntry>());

            DefaultDataSetMergeHandler = new DataSetDefaultMergeHandler();
            DefaultRowMergeHandler = new DataRowDefaultMergeHandler();
            DataTypeDefaultValueProvider = new MetadataDefaultsProvider();

            // The most common interactive scenario: refresh while preserving local pending changes.
            MergeMode = MergeMode.RefreshPreservingLocalChanges;

            RowMergeHandlers = new Dictionary<string, IRowMergeHandler>();
            ExcludeTablesFromMerge = new List<string>();
            ExcludeTablesFromRowDeletion = new List<string>();
            OverriddenPrimaryKeyNames = new Dictionary<string, List<string>>();
        }
        #endregion

        #region Public Properties
        public IDictionary<string, IDataSetMergeHandler> DataSetMergeHandlers { get; private set; }

        public IDataSetMergeResult DataSetMergeResult { get; private set; }

        public IDataSetMergeHandler DefaultDataSetMergeHandler { get; set; }

        public IRowMergeHandler DefaultRowMergeHandler { get; set; }

        public IDataTypeDefaultValueProvider DataTypeDefaultValueProvider { get; private set; }

        public List<string> ExcludeTablesFromMerge { get; private set; }

        public List<string> ExcludeTablesFromRowDeletion { get; private set; }

        public MergeMode MergeMode { get; set; }

        public IDictionary<string, List<string>> OverriddenPrimaryKeyNames { get; private set; }

        public IDictionary<string, IRowMergeHandler> RowMergeHandlers { get; private set; }
        #endregion

        #region Public Methods
        public IDataSetMergeHandler GetDataSetMergeHandler(string? dataSetMergeHandlerKey)
        {
            if (string.IsNullOrWhiteSpace(dataSetMergeHandlerKey))
            {
                return DefaultDataSetMergeHandler;
            }

            if (DataSetMergeHandlers.TryGetValue(dataSetMergeHandlerKey, out IDataSetMergeHandler? handler))
            {
                return handler;
            }

            return DefaultDataSetMergeHandler;
        }

        public List<string> GetPrimaryKeyColumnNames(IDataTable dataTable)
        {
            if (OverriddenPrimaryKeyNames.TryGetValue(dataTable.TableName, out List<string>? overridden) && overridden != null && overridden.Count > 0)
            {
                return overridden;
            }

            // IDataTable.PrimaryKeys is already authoritative in this refactored design.
            return new List<string>(dataTable.PrimaryKeys);
        }

        public IRowMergeHandler GetRowMergeHandler(string tableName)
        {
            if (RowMergeHandlers.TryGetValue(tableName, out IRowMergeHandler? handler))
            {
                return handler;
            }

            return DefaultRowMergeHandler;
        }
        #endregion
    }
}
