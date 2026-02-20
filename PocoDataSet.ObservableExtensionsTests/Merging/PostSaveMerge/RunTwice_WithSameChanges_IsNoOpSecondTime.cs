using System;

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
        /// Verifies WhenRunTwice WithSameChangeset IsNoOpSecondTime in PostSaveMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void RunTwice_WithSameChanges_IsNoOpSecondTime()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey = Guid.NewGuid();

            IObservableDataRow row = t.AddNewRow();
            row[SpecialColumnNames.CLIENT_KEY] = clientKey;
            row["Name"] = "Changed";

            // Post-save snapshot (server confirms values + can assign identity)
            IDataSet postSave = DataSetFactory.CreateDataSet();
            IDataTable rt = postSave.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow saved = rt.AddNewRow();
            saved["Id"] = 1;
            saved[SpecialColumnNames.CLIENT_KEY] = clientKey;
            saved["Name"] = "Changed";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act 1
            current.DoPostSaveMerge(postSave, options);

            // Clear results so we can observe the second run cleanly
            options.ObservableDataSetMergeResult.Clear();

            // Act 2 (same post-save snapshot again)
            current.DoPostSaveMerge(postSave, options);

            // Assert: second run is a no-op
            Assert.Empty(options.ObservableDataSetMergeResult.AddedObservableDataRows);
            Assert.Empty(options.ObservableDataSetMergeResult.UpdatedObservableDataRows);
            Assert.Empty(options.ObservableDataSetMergeResult.DeletedObservableDataRows);
        }
    }
}