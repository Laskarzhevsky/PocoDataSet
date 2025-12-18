namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data type default value provider
    /// </summary>
    public interface IDataTypeDefaultValueProvider
    {
        #region Methods
        /// <summary>
        /// Gets default value
        /// </summary>
        /// <param name="dataType">Data type</param>
        /// <param name="isNullable">Flag indicating whether data type is nullable</param>
        /// <returns></returns>
        object? GetDefaultValue(string? dataType, bool isNullable);
        #endregion
    }
}
