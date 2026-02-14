using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal sealed class RefreshMergeStrategy : ITableMergeStrategy
    {
        public void Execute(DataTableDefaultMergeHandler handler, MergeContext context)
        {
            handler.MergeRefresh(context.CurrentDataTable, context.RefreshedDataTable, context.MergeOptions);
        }
    }
}
