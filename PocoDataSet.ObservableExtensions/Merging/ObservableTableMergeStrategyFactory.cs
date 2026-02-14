using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    internal static class ObservableTableMergeStrategyFactory
    {
        public static IObservableTableMergeStrategy Create(ObservableMergeContext context)
        {
            if (context.Policy.IsFullReload)
            {
                return new FullReloadMergeStrategy();
            }

            return new KeyedMergeStrategy();
        }
    }
}
