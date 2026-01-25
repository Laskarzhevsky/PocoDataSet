using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.Extensions.Relations;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Validates referential integrity based on <see cref="IDataSet.Relations"/>.
        /// The current POCO DataSet core model stores relations as metadata only; this extension performs the
        /// integrity checks when you need them (e.g., before SaveChanges, before AcceptChanges, etc.).
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="options">Validation options (optional)</param>
        /// <returns>List of violations (empty if none)</returns>
        public static List<RelationIntegrityViolation> ValidateRelations(this IDataSet? dataSet, RelationValidationOptions? options = null)
        {
            List<RelationIntegrityViolation> violations = new List<RelationIntegrityViolation>();

            if (dataSet == null)
            {
                return violations;
            }

            if (dataSet.Relations == null || dataSet.Relations.Count == 0)
            {
                return violations;
            }

            RelationValidationOptions actualOptions = options ?? new RelationValidationOptions();

            for (int i = 0; i < dataSet.Relations.Count; i++)
            {
                IDataRelation relation = dataSet.Relations[i];
                if (relation == null)
                {
                    continue;
                }

                RelationValidator.ValidateRelationDefinition(dataSet, relation, actualOptions, violations, out IDataTable? parentTable, out IDataTable? childTable);
                if (parentTable == null || childTable == null)
                {
                    continue;
                }

                // Orphan check: for each child row (not deleted by default), ensure matching parent exists.
                RelationValidator.ValidateOrphanChildRows(relation, parentTable, childTable, actualOptions, violations);

                // Delete-restrict check: if parent is Deleted, ensure no non-deleted children exist.
                if (actualOptions.EnforceDeleteRestrict)
                {
                    RelationValidator.ValidateDeletedParentsHaveNoChildren(relation, parentTable, childTable, actualOptions, violations);
                }
            }

            return violations;
        }
        #endregion
    }
}
