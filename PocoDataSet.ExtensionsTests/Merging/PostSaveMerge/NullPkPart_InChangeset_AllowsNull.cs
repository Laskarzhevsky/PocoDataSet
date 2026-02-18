using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Documents and locks CURRENT PostSave behavior when the server changeset contains a **null/DBNull PK part**
    /// for a table that declares that column as part of the primary key.
    ///
    /// Why this matters:
    /// - For refresh merges, we typically *reject* null PKs because they break stable identity.
    /// - PostSave, however, may receive incomplete server responses in the wild (or buggy stored procedures),
    ///   and the current implementation does NOT validate PK parts for null/DBNull before applying values.
    ///
    /// Scenario:
    /// - CURRENT has a client Added row correlated by __ClientKey.
    /// - Table has a COMPOSITE PK: (DeptId, LocationId).
    /// - SERVER PostSave changeset returns the same row by __ClientKey but leaves one PK part null/DBNull.
    ///
    /// Expected behavior (current implementation):
    /// - PostSave correlates by __ClientKey, applies server values (including null/DBNull), and AcceptChanges.
    /// - The row ends Unchanged (because AcceptChanges was called).
    /// - No exception is thrown by PostSave.
    ///
    /// How this test proves it:
    /// - Arrange creates the current Added row with temporary PK parts.
    /// - Arrange creates the server row with one PK part = DBNull.Value.
    /// - Act executes PostSave merge.
    /// - Assert verifies the PK part became null/DBNull and the row was accepted (Unchanged).
    ///
    /// Notes:
    /// - This test intentionally locks the *existing behavior*, even though a future contract might choose to throw.
    /// - This file contains exactly one test method: NullPkPart_InChangeset_AllowsNull.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void NullPkPart_InChangeset_AllowsNull()
        {
            // Arrange (CURRENT)
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("DeptId", DataTypeNames.INT32, false, true);
            t.AddColumn("LocationId", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey = Guid.NewGuid();

            IDataRow currentRow = new DataRow();
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentRow["DeptId"] = -1;
            currentRow["LocationId"] = -1;
            currentRow["Name"] = "Customer Service";
            t.AddRow(currentRow);

            Assert.Equal(DataRowState.Added, currentRow.DataRowState);

            // Arrange (CHANGESET)
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable ct = changeset.AddNewTable("Department");
            ct.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            ct.AddColumn("DeptId", DataTypeNames.INT32, false, true);
            ct.AddColumn("LocationId", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            IDataRow serverRow = new DataRow();
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;

            // Server assigns one PK part but leaves the other NULL/DBNull (problematic, but tolerated today).
            serverRow["DeptId"] = 10;
            serverRow["LocationId"] = DBNull.Value;
            serverRow["Name"] = "Customer Service";

            ct.AddRow(serverRow);
            serverRow.SetDataRowState(DataRowState.Added);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoPostSaveMerge(changeset, options);

            // Assert
            // The row instance remains the same.
            Assert.Same(currentRow, t.Rows[0]);

            // DeptId was applied.
            Assert.Equal(10, (int)t.Rows[0]["DeptId"]!);

            // LocationId became DBNull (as applied by the merger).
            Assert.True(Convert.IsDBNull(t.Rows[0]["LocationId"]));

            // PostSave AcceptChanges completes, so the row ends Unchanged.
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);
        }
    }
}
