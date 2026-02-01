using System;
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
        /// Tries to find a row by primary key values by scanning table rows.
        /// Works for single or composite primary keys.
        /// </summary>
        public static bool TryFindRowByPrimaryKey(this IDataTable? dataTable, List<string> primaryKeyColumnNames, object?[] primaryKeyValues, out IDataRow? foundRow)
        {
            foundRow = null;

            if (dataTable == null)
            {
                return false;
            }

            if (primaryKeyColumnNames == null || primaryKeyColumnNames.Count == 0)
            {
                return false;
            }

            if (primaryKeyValues == null)
            {
                return false;
            }

            if (primaryKeyColumnNames.Count != primaryKeyValues.Length)
            {
                return false;
            }

            foreach (IDataRow dataRow in dataTable.Rows)
            {
                bool matches = true;

                for (int i = 0; i < primaryKeyColumnNames.Count; i++)
                {
                    string columnName = primaryKeyColumnNames[i];

                    object? rowValue = dataRow.GetDataFieldValue<object?>(columnName);
                    object? expectedValue = primaryKeyValues[i];

                    rowValue = NormalizeDbNull(rowValue);
                    expectedValue = NormalizeDbNull(expectedValue);

                    if (!AreEqual(rowValue, expectedValue))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    foundRow = dataRow;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Convenience overload for single-column primary keys.
        /// </summary>
        public static bool TryFindRowByPrimaryKey(this IDataTable? dataTable, string primaryKeyColumnName, object? primaryKeyValue, out IDataRow? foundRow)
        {
            foundRow = null;

            if (string.IsNullOrWhiteSpace(primaryKeyColumnName))
            {
                return false;
            }

            List<string> columns = new List<string>();
            columns.Add(primaryKeyColumnName);

            object?[] values = new object?[1];
            values[0] = primaryKeyValue;

            return TryFindRowByPrimaryKey(dataTable, columns, values, out foundRow);
        }

        static object? NormalizeDbNull(object? value)
        {
            if (value == null)
            {
                return null;
            }

            if (Convert.IsDBNull(value))
            {
                return null;
            }

            return value;
        }

        static bool AreEqual(object? left, object? right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.Equals(right);
        }
        #endregion
    }
}
