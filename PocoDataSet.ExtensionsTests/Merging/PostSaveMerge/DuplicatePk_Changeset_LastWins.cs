using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks PostSave behavior when the server changeset contains **duplicate rows with the same PK**.
    ///
    /// Background:
    /// - The PostSave table merger iterates changeset rows in order and applies each one independently.
    /// - There is no explicit "duplicate PK detection" in PostSave today.
    ///
    /// Scenario:
    /// - CURRENT contains one existing (Unchanged) row with Id=10.
    /// - CHANGESET contains TWO rows (both Modified) with Id=10, but different Name values.
    ///
    /// Expected behavior (current implementation):
    /// - PostSave applies the first update, then applies the second update to the same target row.
    /// - The final value on the current row reflects the LAST changeset row processed.
    /// - MergeResult records BOTH updates (two UpdatedDataRows entries referencing the same row).
    ///
    /// How this test proves it:
    /// - Arrange sets up current row and two changeset rows with the same PK.
    /// - Act runs PostSave merge.
    /// - Assert checks the final Name, the row state, and the merge result entry count.
    ///
    /// Notes:
    /// - This locks existing behavior. If you later decide to throw on duplicates, this test will intentionally fail,
    ///   prompting you to update the contract + implementation together.
    /// - This file contains exactly one test method: DuplicatePk_Changeset_LastWins.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void DuplicatePk_Changeset_LastWins()
        {
            // Arrange (CURRENT)
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(t.Columns);
            currentRow["Id"] = 10;
            currentRow["Name"] = "Before";
            t.AddLoadedRow(currentRow);

            Assert.Equal(DataRowState.Unchanged, currentRow.DataRowState);

            // Arrange (CHANGESET) - two updates for the same PK
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable ct = changeset.AddNewTable("Department");
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            IDataRow firstUpdate = DataRowExtensions.CreateRowFromColumns(ct.Columns);
            firstUpdate["Id"] = 10;
            firstUpdate["Name"] = "First";
            ct.AddLoadedRow(firstUpdate);
            firstUpdate.AcceptChanges();

            // Make it a true UPDATE (value differs from the loaded baseline)
            firstUpdate["Name"] = "First (server)";
            Assert.Equal(DataRowState.Modified, firstUpdate.DataRowState);

            IDataRow secondUpdate = DataRowExtensions.CreateRowFromColumns(ct.Columns);
            secondUpdate["Id"] = 10;
            secondUpdate["Name"] = "Second";
            ct.AddLoadedRow(secondUpdate);
            secondUpdate.AcceptChanges();

            // Make it a true UPDATE (value differs from the loaded baseline)
            secondUpdate["Name"] = "Second (server)";
            Assert.Equal(DataRowState.Modified, secondUpdate.DataRowState);

            MergeOptions options = new MergeOptions();
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Act
            current.DoPostSaveMerge(changeset, options);

            // Assert
            // The last update wins.
            Assert.Equal("Second (server)", (string)t.Rows[0]["Name"]!);

            // PostSave completes by AcceptChanges.
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);

            // MergeResult recorded TWO updates for the same row (current behavior).
            Assert.Equal(2, result.UpdatedDataRows.Count);
            Assert.Equal(0, result.AddedDataRows.Count);
            Assert.Equal(0, result.DeletedDataRows.Count);
        }
    }
}