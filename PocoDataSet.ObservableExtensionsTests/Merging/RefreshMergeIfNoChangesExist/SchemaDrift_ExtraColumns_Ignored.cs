using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class RefreshMergeIfNoChangesExist
    {
        /// <summary>
        /// Locks schema drift behavior for observable RefreshIfNoChangesExist when REFRESHED has extra columns. Current schema remains authoritative; extra server columns are ignored.
        /// </summary>
        [Fact]
        public void SchemaDrift_ExtraColumns_Ignored()
        {
            // Arrange CURRENT with Id+Name only.
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow c1 = t.AddNewRow();
            c1["Id"] = 1;
            c1["Name"] = "One";
            c1.AcceptChanges();

            // REFRESHED includes an extra column "Extra".
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);
            rt.AddColumn("Extra", DataTypeNames.STRING);

            IDataRow r1 = rt.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "One (server)";
            r1["Extra"] = "Ignored";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: still only the two user columns (Id, Name); Extra is not added.
            Assert.Equal(2, MergeTestingHelpers.CountUserColumns(t));
            Assert.Equal("One (server)", t.Rows[0]["Name"]);

        }
    }
}
