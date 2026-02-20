using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using System;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    /// <summary>
    /// Composite primary key matrix (Observable).
    /// NOTE: Based on observed behavior, RefreshIfNoChangesExist does not throw when refreshed composite PK contains null/DBNull,
    /// while RefreshPreservingLocalChanges does throw for null PK parts.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void CorrelatesOnlyWhenAllCompositePkPartsMatch()
        {
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            MergeTestingHelpers.AddAcceptedRow(t, 1, "X", "One-X");
            MergeTestingHelpers.AddAcceptedRow(t, 1, "Y", "One-Y");

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = MergeTestingHelpers.AddCompositePkTable(refreshed);

            MergeTestingHelpers.AddLoadedRow(rt, 1, "X", "One-X (updated)");
            MergeTestingHelpers.AddLoadedRow(rt, 1, "Z", "One-Z");

            ObservableMergeOptions options = new ObservableMergeOptions();

            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            Assert.Single(options.ObservableDataSetMergeResult.UpdatedObservableDataRows);
            Assert.Single(options.ObservableDataSetMergeResult.DeletedObservableDataRows);
            Assert.Single(options.ObservableDataSetMergeResult.AddedObservableDataRows);

            Assert.Equal(2, t.Rows.Count);

            Assert.True(MergeTestingHelpers.ContainsCompositePk(t, 1, "X"));
            Assert.True(MergeTestingHelpers.ContainsCompositePk(t, 1, "Z"));
            Assert.False(MergeTestingHelpers.ContainsCompositePk(t, 1, "Y"));
        }
    }
}
