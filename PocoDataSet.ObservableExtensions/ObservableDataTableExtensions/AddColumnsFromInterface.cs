using System;

using PocoDataSet.Extensions;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data table extension methods
    /// </summary>
    public static partial class ObservableDataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Adds columns from interface type to observable table (delegates to inner table)
        /// </summary>
        /// <typeparam name="TInterface">Interface type</typeparam>
        /// <param name="observableDataTable">Observable data table</param>
        public static void AddColumnsFromInterface<TInterface>(this IObservableDataTable? observableDataTable)
        {
            if (observableDataTable == null)
            {
                return;
            }

            observableDataTable.InnerDataTable.AddColumnsFromInterface(typeof(TInterface));
        }
        #endregion
    }
}
