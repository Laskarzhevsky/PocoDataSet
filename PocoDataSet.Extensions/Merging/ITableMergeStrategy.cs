using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal interface ITableMergeStrategy
    {
        void Execute(DataTableDefaultMergeHandler handler, MergeContext context);
    }
}
