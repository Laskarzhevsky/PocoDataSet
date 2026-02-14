using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal sealed class PostSaveMergeStrategy : ITableMergeStrategy
    {
        public void Execute(DataTableDefaultMergeHandler handler, MergeContext context)
        {
            handler.MergePostSave(context.CurrentDataTable, context.RefreshedDataTable, context.MergeOptions);
        }
    }
}
