using System;
using System.Collections;
using System.Collections.Generic;

using PocoDataSet.IObjectData;

namespace PocoDataSet.ObjectData
{
    /// <summary>
    /// Default implementation of a strongly typed object table.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public sealed class ObjectTable<T> : IObjectTable<T>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ObjectTable class.
        /// </summary>
        /// <param name="name">Table name</param>
        public ObjectTable(string name)
        {
            Name = name;
            Items = new List<T>();
        }

        /// <summary>
        /// Initializes a new instance of the ObjectTable class.
        /// </summary>
        /// <param name="name">Table name</param>
        /// <param name="items">Initial items</param>
        public ObjectTable(string name, List<T>? items)
        {
            Name = name;
            Items = items ?? new List<T>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets table name
        /// IObjectTable interface implementation
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Gets strongly typed items
        /// IObjectTable interface implementation
        /// </summary>
        public List<T> Items
        {
            get;
        }

        /// <summary>
        /// Gets CLR item type
        /// IObjectTable interface implementation
        /// </summary>
        public Type ItemType
        {
            get
            {
                return typeof(T);
            }
        }

        /// <summary>
        /// Gets items as an untyped list
        /// IObjectTable interface implementation
        /// </summary>
        IList IObjectTable.UntypedItems
        {
            get
            {
                return Items;
            }
        }
        #endregion
    }
}
