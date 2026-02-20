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
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies Throws WhenRefreshedHasDuplicateCompositePrimaryKeys in RefreshMergePreservingLocalChanges merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void Throws_ServerHasDuplicateCompositePKs()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable currentT = current.AddNewTable("EmployeeRole");
            currentT.AddColumn("EmployeeId", DataTypeNames.INT32, false, true);
            currentT.AddColumn("RoleId", DataTypeNames.INT32, false, true);
            currentT.AddColumn("Name", DataTypeNames.STRING);

            // Current can be empty; we just want refreshed validation to run.
            currentT.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedT = refreshed.AddNewTable("EmployeeRole");
            refreshedT.AddColumn("EmployeeId", DataTypeNames.INT32, false, true);
            refreshedT.AddColumn("RoleId", DataTypeNames.INT32, false, true);
            refreshedT.AddColumn("Name", DataTypeNames.STRING);

            DataRow a = new DataRow();
            a["EmployeeId"] = 1;
            a["RoleId"] = 10;
            a["Name"] = "A";
            refreshedT.AddRow(a);

            DataRow b = new DataRow();
            b["EmployeeId"] = 1;
            b["RoleId"] = 10; // duplicate composite PK
            b["Name"] = "B";
            refreshedT.AddRow(b);

            refreshedT.AcceptChanges();

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act / Assert
            Assert.Throws<InvalidOperationException>(delegate
            {
                current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            });
        }
    }
}