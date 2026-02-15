using System;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Creates observable merge policies for internal validation/behavior switches.
    /// </summary>
    public static class ObservableMergePolicyFactory
    {
        public static IObservableMergePolicy Create(MergeMode mergeMode)
        {
            switch (mergeMode)
            {
                case MergeMode.RefreshIfNoChangesExist:
                    return new RefreshIfNoChangesExistMergePolicy();

                case MergeMode.PostSave:
                    return new PostSaveMergePolicy();

                case MergeMode.Replace:
                    return new ReplaceMergePolicy();

                case MergeMode.RefreshPreservingLocalChanges:
                    return new RefreshPreservingLocalChangesMergePolicy();

                default:
                    throw new ArgumentOutOfRangeException(nameof(mergeMode));
            }
        }
    }
}
