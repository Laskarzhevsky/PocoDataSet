using System.Collections.Generic;

namespace PocoDataSet.IObjectData
{
    /// <summary>
    /// Represents a strongly typed object table.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public interface IObjectTable<T> : IObjectTable
    {
        #region Properties
        /// <summary>
        /// Gets strongly typed items
        /// </summary>
        List<T> Items
        {
            get;
        }
        #endregion
    }
}
