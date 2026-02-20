using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class RefreshMergeIfNoChangesExist
    {
        /// <summary>
        /// Unlike the non-observable implementation, the observable RefreshIfNoChangesExist merge rejects duplicate primary keys in the refreshed snapshot (ambiguity). This test locks that it throws InvalidOperationException.
        /// </summary>
        [Fact]
        public void DuplicatePkInSnapshot_Throws()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow c1 = t.AddNewRow();
            c1["Id"] = 1;
            c1["Name"] = "One";
            c1.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            // Duplicate Id=1 appears twice in snapshot.
            IDataRow r1 = rt.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "One (server A)";

            IDataRow r1b = rt.AddNewRow();
            r1b["Id"] = 1;
            r1b["Name"] = "One (server B)";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                delegate
                {
                    current.DoRefreshMergeIfNoChangesExist(refreshed, options);
                });

        }
    }
}
