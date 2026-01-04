using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataTableExtensions
    {
        /// <summary>
        /// Copies data from POCO list into data table by adding to table a new row for every item in the list
        /// </summary>
        /// <typeparam name="T">POCO item type</typeparam>
        /// <param name="dataTable">Data table</param>
        /// <param name="pocoItems">POCO items</param>
        public static void CopyFromPocoList<T>(this IDataTable? dataTable, IList<T>? pocoItems)
        {
            if (dataTable == null || pocoItems == null)
            {
                return;
            }

            for (int i = 0; i < pocoItems.Count; i++)
            {
                T item = pocoItems[i];

                IDataRow row = dataTable.AddNewRow();
                row.CopyFromPoco(item);
                row.AcceptChanges();
            }
        }
    }
}
