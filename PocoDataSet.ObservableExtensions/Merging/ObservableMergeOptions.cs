using System.Collections.Generic;
using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Provides observable merge options functionality
    /// </summary>
    public class ObservableMergeOptions : IObservableMergeOptions
    {
        public ObservableMergeOptions()
        {
            ObservableDataSetMergeHandlers = new Dictionary<string, IObservableDataSetMergeHandler>();
            ObservableDataSetMergeResult = new ObservableDataSetMergeResult(
                new List<IObservableDataSetMergeResultEntry>(),
                new List<IObservableDataSetMergeResultEntry>(),
                new List<IObservableDataSetMergeResultEntry>());

            DefaultObservableDataSetMergeHandler = new ObservableDataSetDefaultMergeHandler();
            DefaultObservableRowMergeHandler = new ObservableDataRowDefaultMergeHandler();
            DataTypeDefaultValueProvider = new MetadataDefaultsProvider();

            MergeMode = MergeMode.RefreshPreservingLocalChanges;

            ObservableRowMergeHandlers = new Dictionary<string, IObservableDataRowMergeHandler>();
            ExcludeTablesFromMerge = new List<string>();
            ExcludeTablesFromRowDeletion = new List<string>();
            OverriddenPrimaryKeyNames = new Dictionary<string, List<string>>();
        }

        public IDataTypeDefaultValueProvider DataTypeDefaultValueProvider { get; private set; }

        public IObservableDataSetMergeHandler DefaultObservableDataSetMergeHandler { get; set; }

        public IObservableDataRowMergeHandler DefaultObservableRowMergeHandler { get; set; }

        public List<string> ExcludeTablesFromMerge { get; private set; }

        public List<string> ExcludeTablesFromRowDeletion { get; private set; }

        public MergeMode MergeMode { get; set; }

        public IDictionary<string, IObservableDataSetMergeHandler> ObservableDataSetMergeHandlers { get; private set; }

        public IObservableDataSetMergeResult ObservableDataSetMergeResult { get; private set; }

        public IDictionary<string, IObservableDataRowMergeHandler> ObservableRowMergeHandlers { get; private set; }

        public IDictionary<string, List<string>> OverriddenPrimaryKeyNames { get; private set; }

        public IObservableDataSetMergeHandler GetObservableDataSetMergeHandler(string? observableDataSetMergeHandlerKey)
        {
            if (string.IsNullOrWhiteSpace(observableDataSetMergeHandlerKey))
            {
                return DefaultObservableDataSetMergeHandler;
            }

            if (ObservableDataSetMergeHandlers.TryGetValue(observableDataSetMergeHandlerKey, out IObservableDataSetMergeHandler? handler))
            {
                return handler;
            }

            return DefaultObservableDataSetMergeHandler;
        }

        public List<string> GetPrimaryKeyColumnNames(IDataTable dataTable)
        {
            if (OverriddenPrimaryKeyNames.TryGetValue(dataTable.TableName, out List<string>? overridden) && overridden != null && overridden.Count > 0)
            {
                return overridden;
            }

            return new List<string>(dataTable.PrimaryKeys);
        }

        public IObservableDataRowMergeHandler GetObservableRowMergeHandler(string tableName)
        {
            if (ObservableRowMergeHandlers.TryGetValue(tableName, out IObservableDataRowMergeHandler? handler))
            {
                return handler;
            }

            return DefaultObservableRowMergeHandler;
        }
    }
}
