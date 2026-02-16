using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    internal interface IObservableRowMerger
    {
        bool MergeDataRow(
            string currentObservableDataTableName,
            IDataRow currentDataRow,
            IDataRow refreshedDataRow,
            IReadOnlyList<IColumnMetadata> listOfColumnMetadata,
            IObservableMergeOptions observableMergeOptions);

        bool MergeObservableRow(
            string currentObservableDataTableName,
            IObservableDataRow currentObservableDataRow,
            IDataRow refreshedDataRow,
            IReadOnlyList<IColumnMetadata> listOfColumnMetadata,
            IObservableMergeOptions observableMergeOptions);
    }
}
