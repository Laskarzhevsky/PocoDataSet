using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Represents a floating (sparse) data row that may contain only explicitly provided fields.
    /// Missing fields are treated as "not provided".
    /// </summary>
    public class FloatingDataRow : DataRow, IFloatingDataRow
    {
        /// <summary>
        /// Creates an empty floating row.
        /// </summary>
        public FloatingDataRow() : base()
        {
        }

        /// <summary>
        /// Creates an empty floating row with pre-sized capacity (optional).
        /// </summary>
        internal FloatingDataRow(int capacity) : base(capacity)
        {
        }
    }
}
