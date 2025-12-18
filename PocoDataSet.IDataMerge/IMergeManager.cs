using System;

namespace PocoDataSet.IDataMerge
{
    /// <summary>
    /// Defines generic merge manager functionality.
    /// </summary>
    /// <typeparam name="TTarget">Target being merged.</typeparam>
    /// <typeparam name="TContext">Context required by merge operations.</typeparam>
    public interface IMergeManager<TTarget, TContext>
    {
        /// <summary>
        /// Initializes merge.
        /// </summary>
        /// <param name="target">Target being merged.</param>
        /// <param name="context">Merge context.</param>
        void InitializeMerge(TTarget target, TContext context);

        /// <summary>
        /// Handles merge.
        /// </summary>
        /// <param name="target">Target being merged.</param>
        /// <param name="context">Merge context.</param>
        void HandleMerge(TTarget target, TContext context);

        /// <summary>
        /// Finalizes merge.
        /// </summary>
        /// <param name="target">Target being merged.</param>
        /// <param name="context">Merge context.</param>
        void FinalizeMerge(TTarget target, TContext context);
    }
}
