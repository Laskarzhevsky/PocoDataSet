using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ObservableMergePolicyFactory
    {
        /// <summary>
        /// Creates merge policy
        /// </summary>
        /// <param name="mergeMode">Merge mode</param>
        /// <returns>Created merge policy</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IObservableMergePolicy Create(MergeMode mergeMode)
        {
            switch (mergeMode)
            {
                case MergeMode.Default:
                    return new DefaultMergePolicy();

                case MergeMode.PostSave:
                    return new PostSaveMergePolicy();

                case MergeMode.Replace:
                    return new ReplaceMergePolicy();

                case MergeMode.Refresh:
                    return new RefreshMergePolicy();

                default:
                    throw new ArgumentOutOfRangeException(nameof(mergeMode));
            }
        }
    }
}