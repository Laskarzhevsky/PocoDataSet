using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class ReplaceMerge
    {
        /// <summary>
        /// Ensures Replace merge removes rows that are missing from the refreshed snapshot and rebuilds the row collection: Id=2 is removed, and neither of the original ObservableDataRow instances should remain after the merge.
        /// </summary>
        [Fact]
        public void Delete_RemovesOldInstances()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            MergeTestingHelpers.AddAcceptedIdNameRow(t, 1, "A");
            MergeTestingHelpers.AddAcceptedIdNameRow(t, 2, "B");

            IObservableDataRow beforeRow1 = MergeTestingHelpers.GetObservableRowById(t, 1);
            IObservableDataRow beforeRow2 = MergeTestingHelpers.GetObservableRowById(t, 2);

            // Refreshed snapshot removes Id=2.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = rt.AddNewRow();
            rr1["Id"] = 1;
            rr1["Name"] = "A";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            Assert.True(MergeTestingHelpers.ContainsRowWithId(t, 1));
            Assert.False(MergeTestingHelpers.ContainsRowWithId(t, 2));

            // Replace rebuilds: neither original instance should remain.
            Assert.False(MergeTestingHelpers.ContainsRowInstance(t, beforeRow1));
            Assert.False(MergeTestingHelpers.ContainsRowInstance(t, beforeRow2));
        }
    }
}
