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
        /// Replaces all rows by rows from
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void ReplaceAllRowsByRowsFrom(this IDataTable? currentDataTable, IDataTable? refreshedDataTable, IMergeOptions mergeOptions)
        {
            if (currentDataTable == null || refreshedDataTable == null)
            {
                return;
            }

            currentDataTable.Rows.Clear();

            foreach (IDataRow refreshedDataRow in refreshedDataTable.Rows)
            {
                IDataRow newDataRow = currentDataTable.AddNewRow();
                DataRowExtensions.MergeWith(newDataRow, refreshedDataRow, currentDataTable.TableName, currentDataTable.Columns, mergeOptions);
            }
        }
        #endregion
    }
}
