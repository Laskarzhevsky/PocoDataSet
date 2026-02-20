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
        public partial class PostSaveMerge
    {
        /// <summary>
        /// Verifies DoesNotReplace ObservableDataSetMergeResult Instance in PostSaveMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void NoReplace_DSResult_Instance()
        {
            // Arrange: current observable dataset has a client-created Added row.
            IDataSet inner = DataSetFactory.CreateDataSet();
            IDataTable t = inner.AddNewTable("Department");
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey = Guid.NewGuid();

            DataRow row = new DataRow();
            row[SpecialColumnNames.CLIENT_KEY] = clientKey;
            row["Id"] = 0;
            row["Name"] = "Customer Service";
            t.AddRow(row);
            Assert.Equal(DataRowState.Added, row.DataRowState);

            IObservableDataSet currentObservable = new ObservableDataSet(inner);

            // Arrange: server changeset with same client key and assigned identity.
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable ct = changeset.AddNewTable("Department");
            ct.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            ct.AddColumn("Id", DataTypeNames.INT32);
            ct.AddColumn("Name", DataTypeNames.STRING);

            DataRow serverRow = new DataRow();
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow["Id"] = 10;
            serverRow["Name"] = "Customer Service";
            ct.AddLoadedRow(serverRow);

            ObservableMergeOptions options = new ObservableMergeOptions();
            IObservableDataSetMergeResult expectedResultInstance = options.ObservableDataSetMergeResult;

            // Act
            currentObservable.DoPostSaveMerge(changeset, options);

            // Assert
            Assert.Same(expectedResultInstance, options.ObservableDataSetMergeResult);

            IObservableDataTable mergedTable = currentObservable.Tables["Department"];
            Assert.Equal(1, mergedTable.Rows.Count);
            Assert.Equal(10, (int)mergedTable.Rows[0]["Id"]!);
            Assert.Equal(DataRowState.Unchanged, mergedTable.Rows[0].DataRowState);
        }
    }
}