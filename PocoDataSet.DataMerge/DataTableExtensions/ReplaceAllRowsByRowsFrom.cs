using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.DataMerge
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Replaces all rows by rows from
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        public static void ReplaceAllRowsByRowsFrom(this IDataTable? currentDataTable, IDataTable? refreshedDataTable)
        {
            if (currentDataTable == null || refreshedDataTable == null)
            {
                return;
            }

            currentDataTable.Rows.Clear();

            foreach (IDataRow refreshedDataRow in refreshedDataTable.Rows)
            {
                IDataRow newDataRow = currentDataTable.AddNewRow();
                newDataRow.MergeWith(refreshedDataRow, currentDataTable.Columns);
            }
        }
        #endregion
    }
}
