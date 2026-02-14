using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal static class TableMergeStrategyFactory
    {
        public static ITableMergeStrategy Create(MergeContext context)
        {
            if (context.MergeOptions != null && context.MergeOptions.MergeMode == MergeMode.PostSave)
            {
                return new PostSaveMergeStrategy();
            }

            return new RefreshMergeStrategy();
        }
    }
}
