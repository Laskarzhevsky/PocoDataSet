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
        /// Creates a new POCO instance from a data row by copying row values into writable properties.
        /// Matching between property names and row keys is case-insensitive.
        /// </summary>
        /// <typeparam name="T">POCO type.</typeparam>
        /// <param name="dataRow">Source data row.</param>
        /// <returns>New POCO instance.</returns>
        public static T ToPoco<T>(this IDataRow? dataRow) where T : new()
        {
            T poco = new T();

            if (dataRow == null)
            {
                return poco;
            }

            dataRow.CopyToPoco(poco);
            return poco;
        }
        #endregion
    }
}
