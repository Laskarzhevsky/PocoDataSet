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
        /// Enumerates child rows for a given parent row and relation.
        /// By default, Deleted child rows are not returned.
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="relationName">Relation name</param>
        /// <param name="parentRow">Parent row</param>
        /// <param name="includeDeletedChildren">If true, Deleted child rows are included</param>
        /// <returns>List of child rows (empty if none)</returns>
        public static List<IDataRow> GetChildRows(this IDataSet? dataSet, string? relationName, IDataRow? parentRow, bool includeDeletedChildren = false)
        {
            List<IDataRow> result = new List<IDataRow>();

            if (dataSet == null || parentRow == null)
            {
                return result;
            }

            IDataRelation? relation = RelationNavigation.FindRelation(dataSet, relationName);
            if (relation == null)
            {
                return result;
            }

            if (!dataSet.TryGetTable(relation.ChildTable, out IDataTable? childTable) || childTable == null)
            {
                return result;
            }

            IReadOnlyList<IDataRow> childRows = childTable.Rows;
            if (childRows == null)
            {
                return result;
            }

            for (int i = 0; i < childRows.Count; i++)
            {
                IDataRow childRow = childRows[i];
                if (childRow == null)
                {
                    continue;
                }

                if (!includeDeletedChildren && childRow.DataRowState == DataRowState.Deleted)
                {
                    continue;
                }

                if (RelationNavigation.RowMatchesRelation(parentRow, childRow, relation))
                {
                    result.Add(childRow);
                }
            }

            return result;
        }
        #endregion
    }
}
