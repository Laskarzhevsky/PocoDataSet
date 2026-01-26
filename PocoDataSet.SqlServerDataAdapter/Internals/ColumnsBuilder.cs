using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides columns builder functionality
    /// </summary>
    internal static class ColumnsBuilder
    {
        #region Public Methods
        /// <summary>
        /// Builds excluded columns
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="tableWriteMetadata">Table write metadata</param>
        /// <returns>Built excluded columns</returns>
        public static HashSet<string> BuildExcludedColumns(IDataTable dataTable, TableWriteMetadata tableWriteMetadata)
        {
            HashSet<string> excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Always exclude computed and rowversion
            foreach (string computedColumn in tableWriteMetadata.ComputedColumns)
            {
                excluded.Add(computedColumn);
            }

            foreach (string rowVersionColumn in tableWriteMetadata.RowVersionColumns)
            {
                excluded.Add(rowVersionColumn);
            }

            // Exclude identity on INSERT (and typically on UPDATE too)
            foreach (string identityColumn in tableWriteMetadata.IdentityColumns)
            {
                excluded.Add(identityColumn);
            }

            return excluded;
        }

        /// <summary>
        /// Builds insert output columns
        /// </summary>
        /// <param name="tableWriteMetadata">Table write metadata</param>
        /// <returns>Built insert output columns</returns>
        public static List<string> BuildInsertOutputColumns(TableWriteMetadata tableWriteMetadata)
        {
            List<string> outputColumns = new List<string>();

            List<string> identityColumns = new List<string>(tableWriteMetadata.IdentityColumns);
            identityColumns.Sort(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < identityColumns.Count; i++)
            {
                outputColumns.Add(identityColumns[i]);
            }

            List<string> rowVersionColumns = new List<string>(tableWriteMetadata.RowVersionColumns);
            rowVersionColumns.Sort(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rowVersionColumns.Count; i++)
            {
                outputColumns.Add(rowVersionColumns[i]);
            }

            return outputColumns;
        }

        /// <summary>
        /// Builds update output columns
        /// </summary>
        /// <param name="tableWriteMetadata">Table write metadata</param>
        /// <returns>Built update output columns</returns>
        public static List<string> BuildUpdateOutputColumns(TableWriteMetadata tableWriteMetadata)
        {
            List<string> outputColumns = new List<string>();

            List<string> rowVersionColumns = new List<string>(tableWriteMetadata.RowVersionColumns);
            rowVersionColumns.Sort(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rowVersionColumns.Count; i++)
            {
                outputColumns.Add(rowVersionColumns[i]);
            }

            return outputColumns;
        }
        #endregion
    }
}
