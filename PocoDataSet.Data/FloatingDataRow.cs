using System.ComponentModel;
using System.Text.Json.Serialization;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Represents a floating (sparse) data row that may contain only explicitly provided fields.
    /// Missing fields are treated as "not provided".
    /// </summary>
    public class FloatingDataRow : DataRow, IFloatingDataRow
    {
        #region Constructors
        /// <summary>
        /// Block "new FloatingDataRow()" outside this assembly.
        /// Use row factory / table APIs to create rows.
        /// </summary>
        [JsonConstructor]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [System.Obsolete("Call DataRowFactory.CreateFloating method to create a new floating data row", false)]
        public FloatingDataRow() : base()
        {
        }

        /// <summary>
        /// Creates an empty floating row with pre-sized capacity (optional).
        /// </summary>
        internal FloatingDataRow(int capacity) : base(capacity)
        {
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets row kind discriminator for JSON round-tripping.
        /// </summary>
        public override string RowKind
        {
            get
            {
                return "floating";
            }
            protected set
            {
                // Ignored. See DataRow.RowKind setter.
            }
        }
        #endregion
    }
}