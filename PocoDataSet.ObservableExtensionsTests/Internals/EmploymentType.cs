namespace PocoDataSet.ObservableExtensionsTests
{
    /// <summary>
    /// Provides employment type functionality
    /// </summary>
    internal class EmploymentType : IEmploymentType
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets code
        /// IEmploymentType interface implementation
        /// </summary>
        public string Code
        {
            get; set;
        } = default!;

        /// <summary>
        /// Gets or sets description
        /// IEmploymentType interface implementation
        /// </summary>
        public string? Description
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets identifier
        /// IEmploymentType interface implementation
        /// </summary>
        public int Id
        {
            get; set;
        }
        #endregion
    }
}
