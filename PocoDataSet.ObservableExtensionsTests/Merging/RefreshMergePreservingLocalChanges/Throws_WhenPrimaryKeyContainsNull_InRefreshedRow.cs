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
    /// <summary>
    /// Additional high-value coverage for the observable merge pipeline after the "no MergeMode / no policies" refactor.
    /// These tests focus on invariants that are easy to regress during future edits.
    /// </summary>
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void Throws_WhenPrimaryKeyContainsNull_InRefreshedRow()
        {
            // Arrange
            ObservableDataSet current = MergeTestingHelpers.CreateCurrentObservableDepartmentDataSet();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable t = refreshed.AddNewTable("Department");
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("Id", DataTypeNames.INT32, true, true); // PK, allow null so the row can be constructed
            t.AddColumn("Name", DataTypeNames.STRING);

	            // NOTE:
	            // Assigning `null` to a value-type column may be normalized to the default value
	            // (e.g., 0 for Int32) depending on conversion rules. Use DBNull.Value to represent
	            // a "missing PK value" reliably.
	            DataRow bad = new DataRow();
	            bad[SpecialColumnNames.CLIENT_KEY] = Guid.NewGuid();
	            bad["Id"] = DBNull.Value;
	            bad["Name"] = "Bad";
            t.AddRow(bad);

            t.AcceptChanges();

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergePreservingLocalChanges(refreshed, options));
        }
    }
}
