using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// Locks insert reconciliation: PostSave must be able to correlate a client-created Added row
    /// with the server response via __ClientKey and apply server-assigned identity (PK).
    /// </summary>
    public class PostSaveServerAssignedPrimaryKeyByClientKeyTests
    {
        [Fact]
        public void PostSave_Reconciles_AddedRow_ByClientKey_AndAppliesServerAssignedPrimaryKey()
        {
            // Arrange: current has an Added row with a temporary identity.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey = Guid.NewGuid();

            IDataRow currentRow = new DataRow();
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentRow["Id"] = -1; // temporary client identity
            currentRow["Name"] = "Customer Service";
            t.AddRow(currentRow);

            Assert.Equal(DataRowState.Added, currentRow.DataRowState);

            // Arrange: server PostSave response correlates by the same client key and assigns real PK.
            IDataSet postSave = DataSetFactory.CreateDataSet();
            IDataTable pt = postSave.AddNewTable("Department");
            pt.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            pt.AddColumn("Id", DataTypeNames.INT32, false, true);
            pt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow serverRow = new DataRow();
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow["Id"] = 10;
            serverRow["Name"] = "Customer Service";
            pt.AddRow(serverRow);

            // Ensure this is a proper changeset row (Added).
            serverRow.SetDataRowState(DataRowState.Added);

            MergeOptions options = new MergeOptions();
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Act
            current.DoPostSaveMerge(postSave, options);

            // Assert: row instance preserved.
            Assert.Same(currentRow, t.Rows[0]);

            // Assert: PK updated.
            Assert.Equal(10, (int)t.Rows[0]["Id"]!);
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);

            // Assert: merge result contains the reconciled row entry.
            Assert.True(ContainsRow(result, "Department", currentRow));
        }

        private static bool ContainsRow(IDataSetMergeResult result, string tableName, IDataRow row)
        {
            foreach (IDataSetMergeResultEntry entry in result.UpdatedDataRows)
            {
                if (entry.TableName == tableName && object.ReferenceEquals(entry.DataRow, row))
                {
                    return true;
                }
            }

            foreach (IDataSetMergeResultEntry entry in result.AddedDataRows)
            {
                if (entry.TableName == tableName && object.ReferenceEquals(entry.DataRow, row))
                {
                    return true;
                }
            }

            foreach (IDataSetMergeResultEntry entry in result.DeletedDataRows)
            {
                if (entry.TableName == tableName && object.ReferenceEquals(entry.DataRow, row))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
