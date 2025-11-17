using System;
using System.Collections.Generic;

using PocoDataSet.Data;
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
        /// Clones rows from data table
        /// </summary>
        /// <param name="clonedDataTable">Cloned data table</param>
        /// <param name="dataTable">Data table</param>
        public static void CloneRowsFrom(this IDataTable? clonedDataTable, IDataTable? dataTable)
        {
            if (clonedDataTable == null || dataTable == null)
            {
                return;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                IDictionary<string, object?> sourceRow = dataTable.Rows[i];
                IDataRow targetRow = CreateEmptyRowFrom(sourceRow);
                FillTargetRowFromSourceRow(sourceRow, targetRow);
                clonedDataTable.Rows.Add(targetRow);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Creates empty row from source row
        /// </summary>
        /// <param name="sourceRow">Source row</param>
        /// <returns>Created empty row from source row</returns>
        static IDataRow CreateEmptyRowFrom(IDictionary<string, object?> sourceRow)
        {
            IDataRow newRow = DataRowFactory.CreateEmpty(sourceRow.Count);
            foreach (KeyValuePair<string, object?> keyValuePair in sourceRow)
            {
                if (!newRow.ContainsKey(keyValuePair.Key))
                {
                    newRow.Add(keyValuePair.Key, null);
                }
            }

            return newRow;
        }

        /// <summary>
        /// Fill target row from source row
        /// </summary>
        /// <param name="sourceRow"></param>
        /// <param name="targetRow"></param>
        static void FillTargetRowFromSourceRow(IDictionary<string, object?> sourceRow, IDataRow targetRow)
        {
            foreach (KeyValuePair<string, object?> kv in sourceRow)
            {
                object? value = kv.Value;
                object? cloned;
                bool copied = TryCloneValue(value, out cloned);
                if (!copied)
                {
                    cloned = value;
                }

                if (targetRow.ContainsKey(kv.Key))
                {
                    targetRow[kv.Key] = cloned;
                }
                else
                {
                    targetRow.Add(kv.Key, cloned);
                }
            }
        }

        /// <summary>
        /// Tries to clone source value
        /// </summary>
        /// <param name="sourceValue">Source value</param>
        /// <param name="clonedValue">Cloned value</param>
        /// <returns></returns>
        static bool TryCloneValue(object? sourceValue, out object? clonedValue)
        {
            if (sourceValue == null)
            {
                clonedValue = null;
                return true;
            }

            // Value types (int, long, decimal, DateTime, bool, etc.) copy by value automatically.
            // Strings are immutable; shallow copy is fine.
            Type t = sourceValue.GetType();
            if (t.IsValueType)
            {
                clonedValue = sourceValue;
                return true;
            }

            string? typeName = t.FullName;
            if (typeName != null)
            {
                if (typeName == "System.String")
                {
                    clonedValue = sourceValue;
                    return true;
                }
            }

            // byte[]: clone to avoid aliasing
            byte[]? bytes = sourceValue as byte[];
            if (bytes != null)
            {
                byte[] copy = new byte[bytes.Length];
                int i = 0;
                int n = bytes.Length;
                while (i < n)
                {
                    copy[i] = bytes[i];
                    i = i + 1;
                }
                clonedValue = copy;
                return true;
            }

            // Arrays of primitives: shallow copy of the array object is usually not safe; do element-wise copy.
            Array? arr = sourceValue as Array;
            if (arr != null)
            {
                Type elemType = t.GetElementType() ?? typeof(object);
                int length = arr.Length;
                Array newArr = Array.CreateInstance(elemType, length);
                int i = 0;
                while (i < length)
                {
                    object? elem = arr.GetValue(i);
                    object? elemClone;
                    bool elemCopied = TryCloneValue(elem, out elemClone);
                    if (elemCopied)
                    {
                        newArr.SetValue(elemClone, i);
                    }
                    else
                    {
                        newArr.SetValue(elem, i);
                    }
                    i = i + 1;
                }
                clonedValue = newArr;
                return true;
            }

            // ICloneable path
            ICloneable? cloneable = sourceValue as ICloneable;
            if (cloneable != null)
            {
                object clonedObj = cloneable.Clone();
                clonedValue = clonedObj;
                return true;
            }

            // Fallback: no deep clone available; use same reference
            clonedValue = sourceValue;
            return false;
        }
        #endregion
    }
}
