using System;
using System.Collections;

namespace PocoDataSet.IObjectData
{
    /// <summary>
    /// Represents a non-generic object table contract.
    /// </summary>
    public interface IObjectTable
    {
        #region Properties
        /// <summary>
        /// Gets table name
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets CLR item type stored in the table
        /// </summary>
        Type ItemType
        {
            get;
        }

        /// <summary>
        /// Gets items as an untyped list
        /// </summary>
        IList UntypedItems
        {
            get;
        }
        #endregion
    }
}
