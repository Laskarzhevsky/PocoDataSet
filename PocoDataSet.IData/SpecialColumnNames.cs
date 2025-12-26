namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines special framework column names.
    /// </summary>
    public static class SpecialColumnNames
    {
        /// <summary>
        /// Client-only key used to correlate rows across a changeset roundtrip.
        /// This column must never be written to the database.
        /// </summary>
        public const string CLIENT_KEY = "__ClientKey";
    }
}
