using System;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;
using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservablePostSaveIdentityStabilityAdditionalTests
    {
        [Fact]
        public void PostSave_PropagatesIdentity_AndPreservesObservableRowInstance()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservable = new ObservableDataSet(currentInner);

            IObservableDataTable department = currentObservable.AddNewTable("Department");
            department.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            department.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            department.AddColumn("Name", DataTypeNames.STRING);
            department.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid clientKey = Guid.NewGuid();

            IObservableDataRow currentAdded = department.AddNewRow();
            currentAdded[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentAdded["Name"] = "Engineering";

            // Build post-save snapshot: server assigns identity + rowversion.
            IDataSet postSave = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSave.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow saved = postSaveDepartment.AddNewRow();
            saved["Id"] = 10;
            saved[SpecialColumnNames.CLIENT_KEY] = clientKey;
            saved["Name"] = "Engineering";
            saved["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            currentObservable.DoPostSaveMerge(postSave, options);

            // Assert
            IObservableDataRow? merged = FindRowByClientKey(department, clientKey);
            Assert.NotNull(merged);

            // Critical invariant: the same observable row instance remains (UI binding stability).
            Assert.Same(currentAdded, merged);

            Assert.Equal(10, (int)merged!["Id"]!);
            Assert.Equal("Engineering", (string)merged["Name"]!);
            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, (byte[])merged["RowVersion"]!);
        }

        private static IObservableDataRow? FindRowByClientKey(IObservableDataTable table, Guid clientKey)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                IObservableDataRow row = table.Rows[i];
                if (row[SpecialColumnNames.CLIENT_KEY] is Guid key && key == clientKey)
                {
                    return row;
                }
            }

            return null;
        }
    }
}