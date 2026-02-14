using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal sealed class MergeContext
    {
        public MergeContext(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            CurrentDataTable = currentDataTable;
            RefreshedDataTable = refreshedDataTable;
            MergeOptions = mergeOptions;
        }

        public IDataTable CurrentDataTable { get; private set; }

        public IDataTable RefreshedDataTable { get; private set; }

        public IMergeOptions MergeOptions { get; private set; }
    }
}
