using System;
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
        /// Validates relations and throws an exception if any violation exists.
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="options">Validation options (optional)</param>
        /// <exception cref="InvalidOperationException">Thrown when at least one violation exists</exception>
        public static void EnsureRelationsValid(this IDataSet? dataSet, RelationValidationOptions? options = null)
        {
            List<RelationIntegrityViolation> violations = ValidateRelations(dataSet, options);
            if (violations.Count == 0)
            {
                return;
            }

            // Build readable message without LINQ.
            string message = "Referential integrity validation failed. Violations:" + Environment.NewLine;
            for (int i = 0; i < violations.Count; i++)
            {
                message += "- " + violations[i].ToString() + Environment.NewLine;
            }

            throw new InvalidOperationException(message);
        }
        #endregion
    }
}
