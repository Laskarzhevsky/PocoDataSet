using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Tries to find a parent row for a given child row and relation.
        /// By default, Deleted parent rows are ignored.
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="relationName">Relation name</param>
        /// <param name="childRow">Child row</param>
        /// <param name="ignoreDeletedParents">If true, Deleted parent rows are ignored</param>
        /// <param name="parentRow">Parent row if found</param>
        /// <returns>True if found, otherwise false</returns>
        public static bool TryGetParentRow(this IDataSet? dataSet, string? relationName, IDataRow? childRow, bool ignoreDeletedParents, out IDataRow? parentRow)
        {
            parentRow = null;

            if (dataSet == null || childRow == null)
            {
                return false;
            }

            IDataRelation? relation = RelationNavigation.FindRelation(dataSet, relationName);
            if (relation == null)
            {
                return false;
            }

            if (!dataSet.TryGetTable(relation.ParentTable, out IDataTable? parentTable) || parentTable == null)
            {
                return false;
            }

            IReadOnlyList<IDataRow> parentRows = parentTable.Rows;
            if (parentRows == null)
            {
                return false;
            }

            for (int i = 0; i < parentRows.Count; i++)
            {
                IDataRow candidate = parentRows[i];
                if (candidate == null)
                {
                    continue;
                }

                if (ignoreDeletedParents && candidate.DataRowState == DataRowState.Deleted)
                {
                    continue;
                }

                bool match = true;
                for (int c = 0; c < relation.ParentColumns.Count; c++)
                {
                    object? parentValue = candidate[relation.ParentColumns[c]];
                    object? childValue = childRow[relation.ChildColumns[c]];

                    if (!object.Equals(parentValue, childValue))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    parentRow = candidate;
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
