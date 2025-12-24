using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data row extension methods
    /// </summary>
    public static partial class DataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Updates data field value and applies row state transitions.
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value for data field</param>
        /// <returns>Flag indicating whether data field value was updated</returns>
        public static bool UpdateDataFieldValue(this IDataRow? dataRow, string columnName, object? value)
        {
            if (dataRow == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            // Disallow edits to deleted rows
            if (dataRow.DataRowState == DataRowState.Deleted)
            {
                return false;
            }

            Dictionary<string, object?> dataRowAsDictionary = (Dictionary<string, object?>)dataRow;
            if (!dataRowAsDictionary.ContainsKey(columnName))
            {
                return false;
            }

            bool dataFieldValueUpdated = false;

            object? dataFieldCurrentValue;
            bool dataFieldHasValue = dataRowAsDictionary.TryGetValue(columnName, out dataFieldCurrentValue);
            if (!dataFieldHasValue)
            {
                return false;
            }

            if (dataFieldCurrentValue == null)
            {
                if (value == null)
                {
                    // Nothing to do
                }
                else
                {
                    CreateOriginalValuesSnapshot(dataRow);
                    dataRow[columnName] = value;
                    dataFieldValueUpdated = true;
                }
            }
            else
            {
                if (value == null)
                {
                    CreateOriginalValuesSnapshot(dataRow);
                    dataRow[columnName] = value;
                    dataFieldValueUpdated = true;
                }
                else
                {
                    if (dataFieldCurrentValue.Equals(value))
                    {
                        // Nothing to do
                    }
                    else
                    {
                        CreateOriginalValuesSnapshot(dataRow);
                        dataRow[columnName] = value;
                        dataFieldValueUpdated = true;
                    }
                }
            }

            if (dataFieldValueUpdated)
            {
                DataRow? concreteDataRow = dataRow as DataRow;
                if (concreteDataRow != null)
                {
                    concreteDataRow.MarkAsModified();
                }
                else
                {
                    // Fallback for other IDataRow implementations
                    if (dataRow.DataRowState == DataRowState.Unchanged)
                    {
                        dataRow.DataRowState = DataRowState.Modified;
                    }
                }
            }

            return dataFieldValueUpdated;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Captures baseline (original values) if this is the first change of an unchanged row
        /// </summary>
        /// <param name="dataRow">Data row</param>
        private static void CreateOriginalValuesSnapshot(IDataRow dataRow)
        {
            if (dataRow.DataRowState != DataRowState.Unchanged)
            {
                return;
            }

            if (dataRow.HasOriginalValues)
            {
                return;
            }

            // Baseline capture is implementation-specific.
            // Use concrete DataRow helper to snapshot existing values.
            DataRow? concreteDataRow = dataRow as DataRow;
            if (concreteDataRow != null)
            {
                concreteDataRow.CreateOriginalValuesSnapshot();
            }
        }
        #endregion
    }
}
