using System;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data row extension methods.
    /// </summary>
    public static partial class ObservableDataTableExtensions
    {
        #region Public Methods
        public static bool TryFindRowByPrimaryKey(this IObservableDataTable? observableDataTable, List<string> primaryKeyColumnNames, object?[] primaryKeyValues, out IObservableDataRow? foundRow)
        {
            foundRow = null;
            if (observableDataTable == null)
            {
                return false;
            }

            IDataRow? innerRow;
            bool found = observableDataTable.InnerDataTable.TryFindRowByPrimaryKey(primaryKeyColumnNames, primaryKeyValues, out innerRow);
            if (!found || innerRow == null)
            {
                return false;
            }

            // Map inner IDataRow -> observable row wrapper
            return TryFindObservableRowByInnerRow(observableDataTable, innerRow, out foundRow);
        }

        public static bool TryFindRowByPrimaryKey(this IObservableDataTable? observableDataTable, string primaryKeyColumnName, object? primaryKeyValue, out IObservableDataRow? foundRow)
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

            return TryFindRowByPrimaryKey(observableDataTable, columns, values, out foundRow);
        }

        static bool TryFindObservableRowByInnerRow(IObservableDataTable observableDataTable, IDataRow innerRow, out IObservableDataRow? observableRow)
        {
            observableRow = null;

            foreach (IObservableDataRow observableDataRow in observableDataTable.Rows)
            {
                if (ReferenceEquals(observableDataRow.InnerDataRow, innerRow))
                {
                    observableRow = observableDataRow;
                    return true; 
                }
            }

            return false;
        }
        #endregion
    }
}
