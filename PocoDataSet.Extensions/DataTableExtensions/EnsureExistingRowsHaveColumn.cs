using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Ensures that existing rows have column
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">column name</param>
        static void EnsureExistingRowsHaveColumn(this IDataTable? dataTable, string columnName)
        {
            if (dataTable == null)
            {
                return;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                IDataRow row = dataTable.Rows[i];

                // Only safe for our concrete DataRow: we can write into ValuesJson without changing row state.
                PocoDataSet.Data.DataRow? concreteRow = row as PocoDataSet.Data.DataRow;
                if (concreteRow == null)
                {
                    continue;
                }

                if (!concreteRow.ValuesJson.ContainsKey(columnName))
                {
                    concreteRow.ValuesJson.Add(columnName, null);
                }

                // Keep OriginalValues aligned if the row is already tracking baseline.
                if (concreteRow.HasOriginalValues)
                {
                    Dictionary<string, object?> originalValues = concreteRow.OriginalValuesJson;
                    if (!originalValues.ContainsKey(columnName))
                    {
                        originalValues.Add(columnName, null);
                    }
                }
            }
        }
        #endregion
    }
}
