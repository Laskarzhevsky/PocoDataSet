using System;
using System.Reflection;

using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data row extension methods.
    /// </summary>
    public static partial class ObservableDataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Creates a new POCO instance from an observable data row by copying row values into writable properties.
        /// Matching between property names and row keys is case-insensitive.
        /// </summary>
        /// <typeparam name="T">POCO type</typeparam>
        /// <param name="observableDataRow">Source observable data row</param>
        /// <returns>New POCO instance</returns>
        public static T ToPoco<T>(this IObservableDataRow? observableDataRow) where T : new()
        {
            T instance = new T();
            if (observableDataRow == null)
            {
                return instance;
            }

            observableDataRow.CopyToPoco(instance);
            return instance;
        }
        #endregion
    }
}
