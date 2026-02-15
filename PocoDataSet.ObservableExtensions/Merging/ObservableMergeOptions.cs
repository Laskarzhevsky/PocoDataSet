using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Provides observable merge options functionality.
    /// </summary>
    public sealed class ObservableMergeOptions : IObservableMergeOptions
    {
        public ObservableMergeOptions()
        {
            ObservableDataSetMergeResult = new ObservableDataSetMergeResult(
                new List<IObservableDataSetMergeResultEntry>(),
                new List<IObservableDataSetMergeResultEntry>(),
                new List<IObservableDataSetMergeResultEntry>());

            DataTypeDefaultValueProvider = new MetadataDefaultsProvider();

            ExcludeTablesFromMerge = new List<string>();
            ExcludeTablesFromRowDeletion = new List<string>();
            OverriddenPrimaryKeyNames = new Dictionary<string, List<string>>();
        }

        public IObservableDataSetMergeResult ObservableDataSetMergeResult
        {
            get; private set;
        }

        public IDataTypeDefaultValueProvider DataTypeDefaultValueProvider
        {
            get; private set;
        }

        public List<string> ExcludeTablesFromMerge
        {
            get; private set;
        }

        public List<string> ExcludeTablesFromRowDeletion
        {
            get; private set;
        }

        public IDictionary<string, List<string>> OverriddenPrimaryKeyNames
        {
            get; private set;
        }

        public List<string> GetPrimaryKeyColumnNames(IDataTable dataTable)
        {
            if (OverriddenPrimaryKeyNames.TryGetValue(dataTable.TableName, out List<string>? overridden) &&
                overridden != null &&
                overridden.Count > 0)
            {
                return overridden;
            }

            return new List<string>(dataTable.PrimaryKeys);
        }
    }
}
