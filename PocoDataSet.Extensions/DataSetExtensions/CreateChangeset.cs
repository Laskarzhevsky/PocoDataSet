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

                    // Preserve state
                    targetRow.DataRowState = sourceRow.DataRowState;

                    targetTable.Rows.Add(targetRow);
                }
            }

            return changeset;
        }
        #endregion
    }
}
