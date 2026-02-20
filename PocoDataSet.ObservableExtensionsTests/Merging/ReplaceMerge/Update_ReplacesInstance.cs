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
        /// Locks the identity contract for Replace merge: Replace is allowed to rebuild the row collection, so an updated row is expected to be represented by a different ObservableDataRow/InnerDataRow instance after the merge. The test asserts the old instances are not retained.
        /// </summary>
        [Fact]
        public void Update_ReplacesInstance()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            MergeTestingHelpers.AddAcceptedIdNameRow(t, 1, "A");

            // Capture identity before merge.
            IObservableDataRow beforeRow = MergeTestingHelpers.GetObservableRowById(t, 1);
            IDataRow beforeInner = beforeRow.InnerDataRow;

            // Refreshed snapshot changes the value for Id=1.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = rt.AddNewRow();
            rr1["Id"] = 1;
            rr1["Name"] = "A2";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            IObservableDataRow afterRow = MergeTestingHelpers.GetObservableRowById(t, 1);

            // Replace merge is allowed to rebuild rows; lock that it DOES replace the row instance.
            Assert.NotSame(beforeRow, afterRow);
            Assert.NotSame(beforeInner, afterRow.InnerDataRow);

            // Value comes from refreshed snapshot.
            Assert.Equal("A2", afterRow["Name"]);

            // Old instance should not remain in the table.
            Assert.False(MergeTestingHelpers.ContainsRowInstance(t, beforeRow));
        }
    }
}
