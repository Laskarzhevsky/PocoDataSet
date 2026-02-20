using System;
using System.Collections.Specialized;
using System.ComponentModel;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class PostSaveMerge
    {
        /// <summary>
        /// Ensures that when the merge removes exactly one row from the observable table, the table raises exactly one CollectionChanged(Remove) notification (no Add).
        /// </summary>
        [Fact]
        public void DeletesRow_RaisesCollectionRemoveOnce()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            Guid key1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid key2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            IObservableDataRow r1 = t.AddNewRow();
            r1[SpecialColumnNames.CLIENT_KEY] = key1;
            r1["Id"] = 1;
            r1["Name"] = "A";
            r1.InnerDataRow.AcceptChanges();

            IObservableDataRow r2 = t.AddNewRow();
            r2[SpecialColumnNames.CLIENT_KEY] = key2;
            r2["Id"] = 2;
            r2["Name"] = "B";
            r2.InnerDataRow.AcceptChanges();

            // Mark the second row as Deleted so PostSave merge can finalize it by removing it from the table.
            r2.InnerDataRow.Delete();

            CollectionChangedCounter counter = new CollectionChangedCounter();
            t.CollectionChanged += counter.Handler;

            // Refreshed contains only the first row, so the second row should be removed by the merge.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = rt.AddNewRow();
            rr1[SpecialColumnNames.CLIENT_KEY] = key1;
            rr1["Id"] = 1;
            rr1["Name"] = "A";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoPostSaveMerge(refreshed, options);

            // Assert
            Assert.Equal(0, counter.AddEvents);
            Assert.Equal(1, counter.RemoveEvents);
            Assert.True(MergeTestingHelpers.ContainsRowWithId(t, 1));
            Assert.False(MergeTestingHelpers.ContainsRowWithId(t, 2));
        }
    }
}
