using System;

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
        /// Verifies PreservesCurrentSchema WhenRefreshedIsMissingColumns in ReplaceMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void KeepsCurrentSchema_ServerIsMissingColumns()
        {
            // Arrange
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow existing = currentTable.AddNewRow();
            existing["Id"] = 1;
            existing["Name"] = "Old";
            existing.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            // NOTE: "Name" intentionally missing

            IDataRow r1 = refreshedTable.AddNewRow();
            r1["Id"] = 10;

            // Act
            IObservableMergeOptions options = new ObservableMergeOptions();
            current.DoReplaceMerge(refreshed, options);

            // Assert: user schema is preserved
            Assert.True(MergeTestingHelpers.HasColumn(currentTable, "Id"));
            Assert.True(MergeTestingHelpers.HasColumn(currentTable, "Name"));
            Assert.Equal(2, MergeTestingHelpers.CountUserColumns(currentTable));

            // Assert: rows replaced, missing column becomes default/null
            Assert.Single(currentTable.Rows);
            Assert.Equal(10, (int)currentTable.Rows[0]["Id"]!);

            object? name = currentTable.Rows[0]["Name"];
            Assert.True(name == null || name == DBNull.Value);
        }
    }
}