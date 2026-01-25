using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Provides relation table sorter functionality
    /// </summary>
    internal static class RelationTableSorter
    {
        #region Public Methods
        /// <summary>
        /// Sorts tables by relations
        /// </summary>
        /// <param name="changeset">Changeset with tables</param>
        /// <param name="tableNames">Table names</param>
        /// <returns>Sorted tables by relations</returns>
        public static List<string> SortTablesByRelations(IDataSet changeset, IEnumerable<string> tableNames)
        {
            HashSet<string> present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> originalOrder = new List<string>();

            foreach (string name in tableNames)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (present.Add(name))
                {
                    originalOrder.Add(name);
                }
            }

            // Build graph edges: Parent -> Child
            Dictionary<string, List<string>> outgoing = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> indegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < originalOrder.Count; i++)
            {
                string node = originalOrder[i];
                outgoing[node] = new List<string>();
                indegree[node] = 0;
            }

            List<IDataRelation> relations = changeset.Relations;
            if (relations != null)
            {
                for (int i = 0; i < relations.Count; i++)
                {
                    IDataRelation relation = relations[i];
                    if (relation == null)
                    {
                        continue;
                    }

                    string parent = relation.ParentTableName;
                    string child = relation.ChildTableName;

                    if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(child))
                    {
                        continue;
                    }

                    if (!present.Contains(parent) || !present.Contains(child))
                    {
                        continue;
                    }

                    // Avoid duplicate edges.
                    List<string> children = outgoing[parent];
                    if (!ContainsIgnoreCase(children, child))
                    {
                        children.Add(child);
                        indegree[child] = indegree[child] + 1;
                    }
                }
            }

            // Kahn's algorithm with deterministic ordering based on originalOrder.
            List<string> result = new List<string>(originalOrder.Count);
            List<string> queue = new List<string>();

            for (int i = 0; i < originalOrder.Count; i++)
            {
                string node = originalOrder[i];
                if (indegree[node] == 0)
                {
                    queue.Add(node);
                }
            }

            int guard = 0;

            while (queue.Count > 0)
            {
                // Pick the earliest node by original appearance.
                int bestIndex = 0;
                int bestOrder = IndexOf(originalOrder, queue[0]);

                for (int i = 1; i < queue.Count; i++)
                {
                    int order = IndexOf(originalOrder, queue[i]);
                    if (order < bestOrder)
                    {
                        bestOrder = order;
                        bestIndex = i;
                    }
                }

                string n = queue[bestIndex];
                queue.RemoveAt(bestIndex);

                result.Add(n);

                List<string> outs = outgoing[n];
                for (int i = 0; i < outs.Count; i++)
                {
                    string m = outs[i];
                    indegree[m] = indegree[m] - 1;
                    if (indegree[m] == 0)
                    {
                        queue.Add(m);
                    }
                }

                guard++;
                if (guard > 100000)
                {
                    break;
                }
            }

            // If we have a cycle or something went wrong, fall back to original order.
            if (result.Count != originalOrder.Count)
            {
                return originalOrder;
            }

            return result;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Checks whether list contains value
        /// </summary>
        /// <param name="list">List to inspect</param>
        /// <param name="value">Value to find</param>
        /// <returns>True if list contains value, otherwise false</returns>
        private static bool ContainsIgnoreCase(List<string> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets index of value in the list
        /// </summary>
        /// <param name="list">List to inspect</param>
        /// <param name="value">Value to find</param>
        /// <returns>Index of value in the list</returns>
        private static int IndexOf(List<string> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return int.MaxValue;
        }
        #endregion
    }
}
