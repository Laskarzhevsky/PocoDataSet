using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the PostSave contract for **composite primary keys** when the *client roundtrip correlation* is done via
    /// <see cref="SpecialColumnNames.CLIENT_KEY"/>:
    ///
    /// Scenario:
    /// - CURRENT contains a client-created Added row that does not yet have real server PK values.
    /// - The table uses a COMPOSITE PK (two PK columns).
    /// - SERVER PostSave changeset returns the same logical row, correlated by __ClientKey, with the composite PK
    ///   parts assigned (server identity) and the final persisted values.
    ///
    /// Expected behavior:
    /// - PostSave locates the target row by __ClientKey (even though PK differs) and copies server values into it.
    /// - Composite PK values are updated on the existing row instance.
    /// - AcceptChanges is applied, so the row ends Unchanged (it now represents the saved server snapshot).
    ///
    /// How this test proves it:
    /// - Arrange creates the current Added row with a GUID __ClientKey and temporary PK parts.
    /// - Arrange creates the changeset Added row with the SAME __ClientKey but real PK parts.
    /// - Act runs <c>DoPostSaveMerge</c>.
    /// - Assert verifies:
    ///   (1) the SAME row instance is preserved,
    ///   (2) both PK parts were updated to server values,
    ///   (3) the row is Unchanged,
    ///   (4) merge result recorded the row.
    ///
    /// Notes:
    /// - This file contains exactly one test method: CompositePk_ClientKey_Reconciles.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void CompositePk_ClientKey_Reconciles()
        {
            // Arrange (CURRENT)
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");

            // __ClientKey is used only for correlation; it must never be persisted to the DB.
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            // Composite primary key (two PK columns).
            t.AddColumn("DeptId", DataTypeNames.INT32, false, true);
            t.AddColumn("LocationId", DataTypeNames.INT32, false, true);

            t.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey = Guid.NewGuid();

            // Client-created row: it exists locally and is Added.
            IDataRow currentRow = new DataRow();
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentRow["DeptId"] = -1;       // temporary placeholder
            currentRow["LocationId"] = -1;   // temporary placeholder
            currentRow["Name"] = "Customer Service";
            t.AddRow(currentRow);

            Assert.Equal(DataRowState.Added, currentRow.DataRowState);

            // Arrange (CHANGESET from server)
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable ct = changeset.AddNewTable("Department");
            ct.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            ct.AddColumn("DeptId", DataTypeNames.INT32, false, true);
            ct.AddColumn("LocationId", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            // Server echoes the same logical row by __ClientKey and assigns real composite PK.
            IDataRow serverRow = new DataRow();
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow["DeptId"] = 10;
            serverRow["LocationId"] = 7;
            serverRow["Name"] = "Customer Service";
            ct.AddRow(serverRow);

            // Ensure the row is treated as a "changeset" row by PostSave.
            serverRow.SetDataRowState(DataRowState.Added);

            MergeOptions options = new MergeOptions();
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Act
            // Execute PostSave merge: apply server-returned changeset onto current.
            current.DoPostSaveMerge(changeset, options);

            // Assert (instance stability)
            Assert.Same(currentRow, t.Rows[0]);

            // Assert (composite PK updated from server values)
            Assert.Equal(10, (int)t.Rows[0]["DeptId"]!);
            Assert.Equal(7, (int)t.Rows[0]["LocationId"]!);

            // Assert (state transition: Added -> Unchanged)
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);

            // Assert (result accounting recorded the row)
            Assert.True(ContainsRow(result, "Department", currentRow));
        }
    }
}
