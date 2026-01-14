using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataSetExtensions
    {
        /// <summary>
        /// Alias for Clear method
        /// Clears all rows from all tables in the data set.
        /// Keeps schema (tables and columns) intact.
        /// </summary>
        public static void ClearRows(this IDataSet? dataSet)
        {
            if (dataSet == null)
            {
                return;
            }

            dataSet.Clear();
        }
    }
}
