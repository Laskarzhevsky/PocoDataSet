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
        [Fact]
        public void DoesNotRaiseDataFieldValueChanged_OnExistingRows()
        {
            // Arrange: current observable data set with 2 loaded rows.
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable dept = current.AddNewTable("Department");
            dept.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            dept.AddColumn("Id", DataTypeNames.INT32);
            dept.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow r1 = dept.AddNewRow();
            r1[SpecialColumnNames.CLIENT_KEY] = Guid.Parse("11111111-1111-1111-1111-111111111111");
            r1["Id"] = 1;
            r1["Name"] = "Sales";

            IObservableDataRow r2 = dept.AddNewRow();
            r2[SpecialColumnNames.CLIENT_KEY] = Guid.Parse("22222222-2222-2222-2222-222222222222");
            r2["Id"] = 2;
            r2["Name"] = "HR";

            DataFieldValueChangedCounter fieldChanged = new DataFieldValueChangedCounter();

            // Attach to current row instances. Replace should clear and add new rows,
            // and MUST NOT update fields on these instances.
            for (int i = 0; i < dept.Rows.Count; i++)
            {
                dept.Rows[i].DataFieldValueChanged += fieldChanged.Handler;
            }

            // Refreshed data set with different values.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedDept = refreshed.AddNewTable("Department");
            refreshedDept.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            refreshedDept.AddColumn("Id", DataTypeNames.INT32);
            refreshedDept.AddColumn("Name", DataTypeNames.STRING);

            IDataRow s1 = refreshedDept.AddNewRow();
            s1[SpecialColumnNames.CLIENT_KEY] = Guid.Parse("33333333-3333-3333-3333-333333333333");
            s1["Id"] = 10;
            s1["Name"] = "Engineering";

            IDataRow s2 = refreshedDept.AddNewRow();
            s2[SpecialColumnNames.CLIENT_KEY] = Guid.Parse("44444444-4444-4444-4444-444444444444");
            s2["Id"] = 11;
            s2["Name"] = "Sales - Server";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            Assert.Equal(0, fieldChanged.Count);
        }
    }
}
