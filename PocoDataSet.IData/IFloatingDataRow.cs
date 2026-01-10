using System;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Marker interface for a "floating" (sparse) data row.
    /// A floating row can contain only the fields that are explicitly provided.
    /// Missing fields are treated as "not provided" (distinct from a provided null).
    /// </summary>
    public interface IFloatingDataRow : IDataRow
    {
    }
}
