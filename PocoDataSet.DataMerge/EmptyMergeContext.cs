namespace PocoDataSet.DataMerge
{
    /// <summary>
    /// Represents an empty merge context.
    /// Use when a merge operation does not require any additional context.
    /// </summary>
    public sealed class EmptyMergeContext
    {
        #region Data Fields
        /// <summary>
        /// Holds reference to empty merge context 
        /// </summary>
        public static readonly EmptyMergeContext _emptyMergeContext = new EmptyMergeContext();
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        private EmptyMergeContext()
        {
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets singleton instance.
        /// </summary>
        public EmptyMergeContext Instance
        {
            get
            {
                return _emptyMergeContext;
            }
        }
        #endregion
    }
}
