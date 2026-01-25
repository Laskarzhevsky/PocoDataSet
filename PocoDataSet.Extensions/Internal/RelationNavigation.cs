using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    internal class RelationNavigation
    {
        #region Public Methods
        public static IDataRelation? FindRelation(IDataSet dataSet, string? relationName)
        {
            if (dataSet.Relations == null || dataSet.Relations.Count == 0)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(relationName))
            {
                return null;
            }

            for (int i = 0; i < dataSet.Relations.Count; i++)
            {
                IDataRelation relation = dataSet.Relations[i];
                if (relation != null && string.Equals(relation.RelationName, relationName, StringComparison.OrdinalIgnoreCase))
                {
                    return relation;
                }
            }

            return null;
        }

        public static bool RowMatchesRelation(IDataRow parentRow, IDataRow childRow, IDataRelation relation)
        {
            for (int c = 0; c < relation.ParentColumns.Count; c++)
            {
                object? parentValue = parentRow[relation.ParentColumns[c]];
                object? childValue = childRow[relation.ChildColumns[c]];

                if (!object.Equals(parentValue, childValue))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
