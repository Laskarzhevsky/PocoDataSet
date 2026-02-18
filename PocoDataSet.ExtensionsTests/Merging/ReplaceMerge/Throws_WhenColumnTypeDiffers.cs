using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Replace schema contract matrix (Policy A - industry common): - Current schema is
    /// authoritative. - Refreshed extra columns are ignored. - Refreshed missing columns do not
    /// remove current columns; values for missing columns become null/default on replaced rows. -
    /// Incompatible column type changes are rejected (throw) to avoid silent corruption.
    ///
    /// Scenario:
    /// - Build a CURRENT DataSet (the client-side truth before the merge).
    /// - Build a REFRESHED/CHANGESET DataSet (the server-side snapshot or post-save response).
    /// - Execute Replace merge (replace rows while enforcing current schema contract).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
    /// Notes:
    /// - This file contains exactly one test method: Throws_WhenColumnTypeDiffers.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void Throws_WhenColumnTypeDiffers()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Amount", DataTypeNames.INT32);

            IDataRow c = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c["Id"] = 1;
            c["Amount"] = 10;
            t.AddLoadedRow(c);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);

            // Refreshed column type differs: string instead of int
            rt.AddColumn("Amount", DataTypeNames.STRING);

            IDataRow r = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r["Id"] = 2;
            r["Amount"] = "NOT_AN_INT";
            rt.AddLoadedRow(r);

            MergeOptions options = new MergeOptions();

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => current.DoReplaceMerge(refreshed, options));
        }
    }
}
