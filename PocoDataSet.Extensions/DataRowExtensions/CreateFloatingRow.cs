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
        /// Creates a floating (sparse) row. The row initially contains no fields.
        /// Fields become "provided" only when explicitly set.
        /// </summary>
        /// <param name="initialCapacity">Optional initial capacity for internal storage</param>
        /// <returns>Created floating row</returns>
        public static IFloatingDataRow CreateFloatingRow(int initialCapacity = 0)
        {
            return DataRowFactory.CreateFloating(initialCapacity);
        }
        #endregion
    }
}
