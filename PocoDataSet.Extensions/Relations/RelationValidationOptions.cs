namespace PocoDataSet.Extensions.Relations
{
    /// <summary>
    /// Defines relation validation options.
    /// </summary>
    public class RelationValidationOptions
    {
        #region Public Properties
        /// <summary>
        /// If true, validation also checks that no Deleted parent row has non-deleted child rows (restrict rule).
        /// </summary>
        public bool EnforceDeleteRestrict
        {
            get; set;
        } = true;

        /// <summary>
        /// If true, validation checks that every (non-deleted) child row has a matching parent row.
        /// For sparse changesets, you may want to disable this and only validate relation definitions and delete restrict.
        /// </summary>
        public bool EnforceOrphanChecks
        {
            get; set;
        } = true;

        /// <summary>
        /// If true, child rows in Deleted state are ignored (default).
        /// </summary>
        public bool IgnoreDeletedChildRows
        {
            get; set;
        } = true;

        /// <summary>
        /// If true, relations are validated even when relation tables/columns are missing.
        /// Missing objects will produce InvalidRelationDefinition violations instead of being ignored.
        /// </summary>
        public bool ReportInvalidRelationDefinitions
        {
            get; set;
        } = true;

        /// <summary>
        /// If true, parent rows in Deleted state are treated as non-existing for orphan checks.
        /// </summary>
        public bool TreatDeletedParentAsMissing
        {
            get; set;
        } = true;

        /// <summary>
        /// If true, child rows with any NULL in relation columns are treated as "not participating" in the relation.
        /// This is useful for optional foreign keys.
        /// </summary>
        public bool TreatNullForeignKeysAsNotSet
        {
            get; set;
        } = true;
        #endregion
    }
}