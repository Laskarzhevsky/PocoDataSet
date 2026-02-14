using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    internal sealed class FullReloadMergeStrategy : IObservableTableMergeStrategy
    {
        public void Execute(ObservableDataTableDefaultMergeHandler handler, ObservableMergeContext context)
        {
            handler.MergeObservableDataRowsWithoutPrimaryKeys(
                context.CurrentObservableDataTable,
                context.RefreshedDataTable,
                context.ObservableMergeOptions);
        }
    }
}
