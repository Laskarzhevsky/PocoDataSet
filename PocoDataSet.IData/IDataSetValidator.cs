namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data sets validation functionality
    /// </summary>
    public interface IDataSetValidator
    {
        #region Methods
        /// <summary>
        /// Applies validation rules
        /// </summary>
        /// <param name="dataSet">Data set</param>
        void ApplyValidationRules(IDataSet dataSet);
        #endregion
    }
}
