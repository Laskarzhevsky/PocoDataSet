using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    internal interface IObservableTableMergeStrategy
    {
        void Execute(ObservableDataTableDefaultMergeHandler handler, ObservableMergeContext context);
    }
}
