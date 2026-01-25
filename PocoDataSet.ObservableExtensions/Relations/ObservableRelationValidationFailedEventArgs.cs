using System;
using System.Collections.Generic;
using PocoDataSet.Extensions.Relations;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Provides ObservableRelationValidationFailed event arguments functionality 
    /// </summary>
    public class ObservableRelationValidationFailedEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="handledEventName">The name of the event being handled, used to identify the context in which relation validation is performed.</param>
        /// <param name="relationIntegrityViolation">Relation integrity violations</param>
        public ObservableRelationValidationFailedEventArgs(string handledEventName, IReadOnlyList<RelationIntegrityViolation> relationIntegrityViolation)
        {
            HandledEventName = handledEventName;
            RelationIntegrityViolations = relationIntegrityViolation;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets handled event name
        /// </summary>
        public string HandledEventName
        {
            get;
        }

        /// <summary>
        /// Gets relation integrity violations
        /// </summary>
        public IReadOnlyList<RelationIntegrityViolation> RelationIntegrityViolations
        {
            get;
        }
        #endregion
    }
}
