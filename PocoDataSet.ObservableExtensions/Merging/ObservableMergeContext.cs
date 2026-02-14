using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    internal sealed class ObservableMergeContext
    {
        public ObservableMergeContext(
            IObservableDataTable currentObservableDataTable,
            IDataTable refreshedDataTable,
            IObservableMergeOptions observableMergeOptions,
            IObservableMergePolicy policy)
        {
            CurrentObservableDataTable = currentObservableDataTable;
            RefreshedDataTable = refreshedDataTable;
            ObservableMergeOptions = observableMergeOptions;
            Policy = policy;
        }

        public IObservableDataTable CurrentObservableDataTable { get; private set; }

        public IDataTable RefreshedDataTable { get; private set; }

        public IObservableMergeOptions ObservableMergeOptions { get; private set; }

        public IObservableMergePolicy Policy { get; private set; }
    }
}
