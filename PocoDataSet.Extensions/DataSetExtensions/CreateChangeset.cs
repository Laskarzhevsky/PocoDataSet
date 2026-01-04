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
        /// Creates a "changeset" dataset that contains only rows in Added/Modified/Deleted states.
        /// </summary>
        public static IDataSet? CreateChangeset(this IDataSet? dataSet)
        {
            if (dataSet == null)
            {
                return dataSet;
            }

            IDataSet changeset = DataSetFactory.CreateDataSet();

            foreach (KeyValuePair<string, IDataTable> keyValuePair in dataSet.Tables)
            {
                string tableName = keyValuePair.Key;
                IDataTable sourceTable = keyValuePair.Value;

                // Collect changed rows
                List<IDataRow> changedRows = new List<IDataRow>();
                for (int i = 0; i < sourceTable.Rows.Count; i++)
                {
                    IDataRow row = sourceTable.Rows[i];
                    if (row.DataRowState == DataRowState.Added ||
                        row.DataRowState == DataRowState.Modified ||
                        row.DataRowState == DataRowState.Deleted)
                    {
                        changedRows.Add(row);
                    }
                }

                if (changedRows.Count == 0)
                {
                    continue;
                }

                // Create target table with same schema
                IDataTable targetTable = changeset.AddNewTable(tableName);
                targetTable.AddColumns(sourceTable.Columns);

                // Copy PrimaryKeys metadata (single or composite)
                if (sourceTable.PrimaryKeys != null && sourceTable.PrimaryKeys.Count > 0)
                {
                    List<string> primaryKeys = new List<string>();
                    for (int i = 0; i < sourceTable.PrimaryKeys.Count; i++)
                    {
                        primaryKeys.Add(sourceTable.PrimaryKeys[i]);
                    }

                    targetTable.PrimaryKeys = primaryKeys;
                }

                // Copy rows
                for (int i = 0; i < changedRows.Count; i++)
                {
                    IDataRow sourceRow = changedRows[i];

                    // Create a new row with same columns
                    IDataRow targetRow = DataRowExtensions.CreateRowFromColumns(sourceTable.Columns);
                    foreach (IColumnMetadata columnMetadata in sourceTable.Columns)
                    {
                        string columnName = columnMetadata.ColumnName;
                        if (sourceRow.ContainsKey(columnName))
                        {
                            targetRow[columnName] = sourceRow[columnName];
                        }
                    }

                    // IMPORTANT:
                    // DataTable.AddRow(IDataRow) disallows adding a row in Deleted state.
                    // For Deleted rows we add as Loaded/Unchanged first, then mark Deleted via DeleteRow.
                    if (sourceRow.DataRowState == DataRowState.Deleted)
                    {
                        targetTable.AddLoadedRow(targetRow);
                        targetTable.DeleteRow(targetRow);
                    }
                    else
                    {
                        // Added or Modified: preserve state explicitly, then add
                        targetRow.SetDataRowState(sourceRow.DataRowState);
                        targetTable.AddRow(targetRow);
                    }
                }
            }

            return changeset;
        }
        #endregion
    }
}
