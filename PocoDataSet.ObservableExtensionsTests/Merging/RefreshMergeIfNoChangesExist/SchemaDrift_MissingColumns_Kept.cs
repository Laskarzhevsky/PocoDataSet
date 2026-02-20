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
        /// Locks schema drift behavior for observable RefreshIfNoChangesExist when REFRESHED is missing columns. Current schema remains authoritative: missing columns stay, but refreshed rows have nulls for those columns.
        /// </summary>
        [Fact]
        public void SchemaDrift_MissingColumns_Kept()
        {
            // Arrange CURRENT with Id+Name+Extra (Extra populated).
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn("Extra", DataTypeNames.STRING);

            IObservableDataRow c1 = t.AddNewRow();
            c1["Id"] = 1;
            c1["Name"] = "One";
            c1["Extra"] = "LocalExtra";
            c1.AcceptChanges();

            // REFRESHED lacks "Extra" column.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = rt.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "One (server)";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: Extra column is kept, but its value becomes null for refreshed rows.
            Assert.Equal(3, MergeTestingHelpers.CountUserColumns(t));
            Assert.Equal("One (server)", t.Rows[0]["Name"]);
            Assert.Null(t.Rows[0]["Extra"]);

        }
    }
}
