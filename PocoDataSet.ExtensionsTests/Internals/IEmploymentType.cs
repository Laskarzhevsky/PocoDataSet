namespace PocoDataSet.ExtensionsTests
{
    /// <summary>
    /// Defines employment type functionality
    /// </summary>
    internal interface IEmploymentType
    {
        #region Properties
        /// <summary>
        /// Gets or sets code
        /// </summary>
        string Code
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets description
        /// </summary>
        string? Description
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets identifier
        /// </summary>
        int Id
        {
            get; set;
        }
        #endregion
    }
}
