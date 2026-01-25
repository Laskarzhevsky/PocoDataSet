namespace PocoDataSet.Extensions.Relations
{
    /// <summary>
    /// Defines relation integrity violation kinds.
    /// </summary>
    public enum RelationIntegrityViolationKind
    {
        /// <summary>
        /// Parent row is marked Deleted while non-deleted child rows still exist.
        /// </summary>
        DeletedParentHasChildren,

        /// <summary>
        /// Relation definition is invalid (missing tables/columns, mismatched column count, etc).
        /// </summary>
        InvalidRelationDefinition,

        /// <summary>
        /// Child row references a parent row that does not exist.
        /// </summary>
        OrphanChildRow
    }
}
