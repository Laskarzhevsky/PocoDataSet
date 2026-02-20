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
        /// Locks the identity contract for Replace merge on an identical snapshot: even when values are identical, Replace rebuilds the row collection, so the ObservableDataRow and InnerDataRow instances are expected to be replaced (new instances) while preserving values.
        /// </summary>
        [Fact]
        public void NoOp_ReplacesInstance()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            MergeTestingHelpers.AddAcceptedIdNameRow(t, 1, "A");

            IObservableDataRow beforeRow = MergeTestingHelpers.GetObservableRowById(t, 1);
            IDataRow beforeInner = beforeRow.InnerDataRow;

            // Refreshed snapshot is identical.
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
            IObservableDataRow afterRow = MergeTestingHelpers.GetObservableRowById(t, 1);

            // Even for a no-op, Replace merge rebuilds the row collection (see notification tests).
            Assert.NotSame(beforeRow, afterRow);
            Assert.NotSame(beforeInner, afterRow.InnerDataRow);

            // Value is still correct.
            Assert.Equal("A", afterRow["Name"]);

            // Old instance removed.
            Assert.False(MergeTestingHelpers.ContainsRowInstance(t, beforeRow));
        }
    }
}
