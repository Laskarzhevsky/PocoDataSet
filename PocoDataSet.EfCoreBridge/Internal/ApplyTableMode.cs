namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Defines table modes to apply
    /// </summary>
    internal enum ApplyTableMode
    {
        /// <summary>
        /// All rows
        /// </summary>
        All,

        /// <summary>
        /// Deleted rows only
        /// </summary>
        DeletesOnly,

        /// <summary>
        /// Added and modified rows only
        /// </summary>
        NonDeletesOnly
    }
}
