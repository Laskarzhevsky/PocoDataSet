using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides primary key processor functionality
    /// </summary>
    internal static class PrimaryKeyProcessor
    {
        #region Public Methods
        /// <summary>
        /// Builds primary key text
        /// </summary>
        /// <param name="tableWriteMetadata">Table write metadata</param>
        /// <param name="dataRow">Data row</param>
        /// <returns>Built primary key text</returns>
        public static string BuildPrimaryKeyText(TableWriteMetadata tableWriteMetadata, IDataRow dataRow)
        {
            List<string> parts = new List<string>();

            for (int i = 0; i < tableWriteMetadata.PrimaryKeyColumns.Count; i++)
            {
                string primaryKeyColumnName = tableWriteMetadata.PrimaryKeyColumns[i];
                object? primaryKeyValue;
                if (!dataRow.TryGetValue(primaryKeyColumnName, out primaryKeyValue))
                {
                    dataRow.TryGetOriginalValue(primaryKeyColumnName, out primaryKeyValue);
                }

                string text;
                if (primaryKeyValue == null)
                {
                    text = primaryKeyColumnName + "=null";
                }
                else
                {
                    text = primaryKeyColumnName + "=" + primaryKeyValue.ToString();
                }

                parts.Add(text);
            }

            return string.Join(", ", parts);
        }

        /// <summary>
        /// Validate primary keys existence in table write metadata
        /// </summary>
        /// <param name="tableWriteMetadata">Table write metadata</param>
        /// <param name="tableName">Table name</param>
        /// <exception cref="InvalidOperationException">Thrown if table write metadata does not contain primary keys data</exception>
        public static void ValidatePrimaryKeysExistence(TableWriteMetadata tableWriteMetadata, string tableName)
        {
            if (tableWriteMetadata.PrimaryKeyColumns.Count == 0)
            {
                throw new InvalidOperationException("Table '" + tableName + "' has no primary keys in SQL Server.");
            }
        }
        #endregion
    }
}
