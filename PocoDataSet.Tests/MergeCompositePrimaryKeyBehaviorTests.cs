using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.Tests
{
    public  class MergeCompositePrimaryKeyBehaviorTests
    {
        [Fact]
        public void Merge_WithCompositePrimaryKey_Refresh_UpdatesMatchesAndRemovesMissingUnchangedRow()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("K1", DataTypeNames.INT32, false, true);
            t.AddColumn("K2", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            // Current snapshot: (1,1)=A, (1,2)=B, (2,1)=C
            IDataRow c11 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c11["K1"] = 1;
            c11["K2"] = 1;
            c11["Name"] = "A";
            t.AddLoadedRow(c11);

            IDataRow c12 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c12["K1"] = 1;
            c12["K2"] = 2;
            c12["Name"] = "B";
            t.AddLoadedRow(c12);

            IDataRow c21 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c21["K1"] = 2;
            c21["K2"] = 1;
            c21["Name"] = "C";
            t.AddLoadedRow(c21);

            // Refreshed snapshot: (1,1)=A, (2,1)=C_UPDATED (and missing (1,2))
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("K1", DataTypeNames.INT32, false, true);
            rt.AddColumn("K2", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r11 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r11["K1"] = 1;
            r11["K2"] = 1;
            r11["Name"] = "A";
            rt.AddLoadedRow(r11);

            IDataRow r21 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r21["K1"] = 2;
            r21["K2"] = 1;
            r21["Name"] = "C_UPDATED";
            rt.AddLoadedRow(r21);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;
// Assert: row (1,2) removed, others remain
            Assert.Equal(2, t.Rows.Count);

            bool has11 = false;
            bool has12 = false;
            bool has21 = false;

            string? name11 = null;
            string? name21 = null;

            for (int i = 0; i < t.Rows.Count; i++)
            {
                IDataRow row = t.Rows[i];
                int k1 = (int)row["K1"]!;
                int k2 = (int)row["K2"]!;

                if (k1 == 1 && k2 == 1)
                {
                    has11 = true;
                    name11 = (string)row["Name"]!;
                }
                else if (k1 == 1 && k2 == 2)
                {
                    has12 = true;
                }
                else if (k1 == 2 && k2 == 1)
                {
                    has21 = true;
                    name21 = (string)row["Name"]!;
                }
            }

            Assert.True(has11);
            Assert.False(has12);
            Assert.True(has21);

            Assert.Equal("A", name11);
            Assert.Equal("C_UPDATED", name21);

            Assert.True(result.DeletedDataRows.Count >= 1);

            bool deleted12Found = false;

            for (int i = 0; i < result.DeletedDataRows.Count; i++)
            {
                IDataSetMergeResultEntry entry = result.DeletedDataRows[i];

                if (entry.TableName != "T")
                {
                    continue;
                }

                IDataRow deletedRow = entry.DataRow;

                int k1 = (int)deletedRow["K1"]!;
                int k2 = (int)deletedRow["K2"]!;

                if (k1 == 1 && k2 == 2)
                {
                    deleted12Found = true;
                    break;
                }
            }

            Assert.True(deleted12Found);
        }
    }
}
