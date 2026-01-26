using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Providews table sorter functionalty
    /// </summary>
    internal static class TableSorter
    {
        #region Public Methods
        /// <summary>
        /// Builds ordered list of tables that contain changes (Added/Modified/Deleted),
        /// ordered automatically by SQL Server foreign key relationships.
        /// </summary>
        /// <param name="changeset">Changeset to save</param>
        /// <param name="tablesWithChanges">Tables with changes</param>
        /// <param name="namesOfTablesWithChanges">Names of tables with changes</param>
        /// <param name="foreignKeyEdges">Foreign key edges</param>
        /// <returns>Built ordered tables with changes</returns>
        public static List<IDataTable> BuildOrderedTablesWithChangesByForeignKeys(IDataSet changeset, List<IDataTable> tablesWithChanges, HashSet<string> namesOfTablesWithChanges, List<ForeignKeyEdge> foreignKeyEdges)
        {
            if (tablesWithChanges.Count == 0)
            {
                return tablesWithChanges;
            }

            // Build a stable order map based on incoming list order (used as tie-breaker).
            Dictionary<string, int> orderIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < tablesWithChanges.Count; i++)
            {
                string name = tablesWithChanges[i].TableName;
                if (!orderIndex.ContainsKey(name))
                {
                    orderIndex.Add(name, i);
                }
            }

            List<string> orderedNames = OrderTableNamesByForeignKeys(namesOfTablesWithChanges, foreignKeyEdges, orderIndex);

            Dictionary<string, IDataTable> tableByName = new Dictionary<string, IDataTable>(StringComparer.OrdinalIgnoreCase);
            List<IDataTable> orderedTables = new List<IDataTable>();
            for (int i = 0; i < orderedNames.Count; i++)
            {
                IDataTable? table;
                tableByName.TryGetValue(orderedNames[i], out table);
                if (table != null)
                {
                    orderedTables.Add(table);
                }
            }

            return orderedTables;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets order index
        /// </summary>
        /// <param name="orderIndexes">Order indexes</param>
        /// <param name="tableName">Table name</param>
        /// <returns></returns>
        static int GetOrderIndex(Dictionary<string, int> orderIndexes, string tableName)
        {
            int idx;
            if (orderIndexes.TryGetValue(tableName, out idx))
            {
                return idx;
            }

            return int.MaxValue;
        }

        /// <summary>
        /// Orders table names by foreign keys
        /// </summary>
        /// <param name="tableNames">Table names</param>
        /// <param name="foreignKeyEdges">Foreign key edges</param>
        /// <param name="orderIndex">order index</param>
        /// <returns>Ordered table names by foreign keys</returns>
        public static List<string> OrderTableNamesByForeignKeys(HashSet<string> tableNames, List<ForeignKeyEdge> foreignKeyEdges, Dictionary<string, int> orderIndex)
        {
            // parent -> children adjacency
            Dictionary<string, List<string>> adjacency = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> indegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (string name in tableNames)
            {
                adjacency[name] = new List<string>();
                indegree[name] = 0;
            }

            for (int i = 0; i < foreignKeyEdges.Count; i++)
            {
                ForeignKeyEdge edge = foreignKeyEdges[i];

                // Parent -> Child
                adjacency[edge.PrincipalTableName].Add(edge.DependentTableName);
                indegree[edge.DependentTableName] = indegree[edge.DependentTableName] + 1;
            }

            // Start nodes (indegree 0) - maintain stable order using orderIndex
            List<string> ready = new List<string>();
            foreach (KeyValuePair<string, int> kvp in indegree)
            {
                if (kvp.Value == 0)
                {
                    ready.Add(kvp.Key);
                }
            }

            SortByIncomingOrder(ready, orderIndex);

            List<string> result = new List<string>();

            while (ready.Count > 0)
            {
                string current = ready[0];
                ready.RemoveAt(0);
                result.Add(current);

                List<string> children = adjacency[current];
                for (int i = 0; i < children.Count; i++)
                {
                    string child = children[i];
                    indegree[child] = indegree[child] - 1;
                    if (indegree[child] == 0)
                    {
                        ready.Add(child);
                    }
                }

                SortByIncomingOrder(ready, orderIndex);
            }

            if (result.Count != tableNames.Count)
            {
                // cycle detected
                List<string> remaining = new List<string>();
                foreach (KeyValuePair<string, int> kvp in indegree)
                {
                    if (kvp.Value > 0)
                    {
                        remaining.Add(kvp.Key);
                    }
                }
                SortByIncomingOrder(remaining, orderIndex);

                throw new InvalidOperationException(
                    "Cannot automatically order tables by foreign keys due to a cycle. Tables in cycle: " + string.Join(", ", remaining));
            }

            return result;
        }

        /// <summary>
        /// Sorts by incoming order
        /// </summary>
        /// <param name="list">List to sort</param>
        /// <param name="orderIndex">Order index</param>
        static void SortByIncomingOrder(List<string> list, Dictionary<string, int> orderIndex)
        {
            // Simple stable insertion sort by orderIndex
            for (int i = 1; i < list.Count; i++)
            {
                string key = list[i];
                int keyOrder = GetOrderIndex(orderIndex, key);

                int j = i - 1;
                while (j >= 0 && GetOrderIndex(orderIndex, list[j]) > keyOrder)
                {
                    list[j + 1] = list[j];
                    j--;
                }

                list[j + 1] = key;
            }
        }

        /// <summary>
        /// Checks whether data table has changes
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>True if data table has changes, otherwise false</returns>
        public static bool TableHasChanges(IDataTable dataTable)
        {
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                IDataRow row = dataTable.Rows[i];
                if (row.DataRowState == PocoDataSet.IData.DataRowState.Added ||
                    row.DataRowState == PocoDataSet.IData.DataRowState.Modified ||
                    row.DataRowState == PocoDataSet.IData.DataRowState.Deleted)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
