using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    internal sealed class KeyedMergeStrategy : IObservableTableMergeStrategy
    {
        public void Execute(ObservableDataTableDefaultMergeHandler handler, ObservableMergeContext context)
        {
            handler.MergeKeyed(
                context.CurrentObservableDataTable,
                context.RefreshedDataTable,
                context.ObservableMergeOptions,
                context.Policy);
        }
    }
}
