using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides changeset process functionalty
    /// </summary>
    internal static class ChangesetProcessor
    {
        #region Public Methods
        /// <summary>
        /// Gets names of tables with changes
        /// </summary>
        /// <param name="tablesWithChanges">Tables with changes</param>
        /// <returns>Names of tables with changes</returns>
        public static HashSet<string> GetNamesOfTablesWithChanges(List<IDataTable> tablesWithChanges)
        {
            HashSet<string> namesOfTablesWithChanges = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < tablesWithChanges.Count; i++)
            {
                namesOfTablesWithChanges.Add(tablesWithChanges[i].TableName);
            }

            return namesOfTablesWithChanges;
        }

        /// <summary>
        /// Gets tables with changes
        /// </summary>
        /// <param name="changeset">Changeset to inspect</param>
        /// <returns>Tables with changes</returns>
        public static List<IDataTable> GetTablesWithChanges(IDataSet changeset)
        {
            List<IDataTable> tablesWithChanges = new List<IDataTable>();
            Dictionary<string, IDataTable> tableByName = new Dictionary<string, IDataTable>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, IDataTable> kvp in changeset.Tables)
            {
                IDataTable table = kvp.Value;
                if (TableHasChanges(table))
                {
                    tablesWithChanges.Add(table);
                    if (!tableByName.ContainsKey(table.TableName))
                    {
                        tableByName.Add(table.TableName, table);
                    }
                }
            }

            return tablesWithChanges;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Checks whether data table has changes
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>True if data table has changes, otherwise false</returns>
        static bool TableHasChanges(IDataTable dataTable)
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
