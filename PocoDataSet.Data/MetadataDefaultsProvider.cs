using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Provides metadata defaults provider functionality
    /// </summary>
    public class MetadataDefaultsProvider : IDataTypeDefaultValueProvider
    {
        #region Public Methods
        /// <summary>
        /// Gets default value
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="isNullable">Flag indicating whether data type is nullable</param>
        /// <returns>Default value</returns>
        public object? GetDefaultValue(string? dataType, bool isNullable)
        {
            return MetadataDefaults.GetDefaultValue(dataType, isNullable);
        }
        #endregion
    }
}
