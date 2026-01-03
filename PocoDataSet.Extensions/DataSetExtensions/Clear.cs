using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataSetExtensions
    {
        /// <summary>
        /// Clears all rows from all tables in the data set.
        /// Keeps schema (tables and columns) intact.
        /// </summary>
        public static void Clear(this IDataSet? dataSet)
        {
            if (dataSet == null)
            {
                return;
            }

            foreach (IDataTable table in dataSet.Tables.Values)
            {
                if (table == null)
                {
                    continue;
                }

                // Remove rows via table API (not via Rows collection)
                while (table.Rows.Count > 0)
                {
                    table.RemoveRowAt(table.Rows.Count - 1);
                }
            }
        }
    }
}
