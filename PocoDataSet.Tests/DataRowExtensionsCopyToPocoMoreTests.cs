using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class DataRowExtensionsCopyToPocoMoreTests
    {
        [Fact]
        public void CopyToPoco_WhenRowContainsGuidAsString_Parses_AndAssignsValue()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.STRING);

            IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
            r["Id"] = "4a31857a-e901-4529-abda-a86c68383704";
            t.AddLoadedRow(r);

            PocoWithGuid target = new PocoWithGuid();
            target.Id = new Guid("832a723a-b287-4507-aff9-b71e639d036d");

            // Act
            r.CopyToPoco(target);

            // Assert: CopyToPoco parses Guid from string and assigns it
            Assert.Equal(new Guid("4a31857a-e901-4529-abda-a86c68383704"), target.Id);
        }

        [Fact]
        public void CopyToPoco_WhenTargetPropertyIsNullableEnum_AssignsParsedEnumValue()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Status", DataTypeNames.STRING);

            IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
            r["Status"] = "Active";
            t.AddLoadedRow(r);

            PocoWithNullableEnum target = new PocoWithNullableEnum();
            target.Status = StatusEnum.Inactive;

            // Act
            r.CopyToPoco(target);

            // Assert: CopyToPoco parses enum (case-insensitive) and assigns it
            Assert.Equal(StatusEnum.Active, target.Status);
        }

        private sealed class PocoWithGuid
        {
            public Guid Id
            {
                get; set;
            }
        }

        private enum StatusEnum
        {
            Inactive = 0,
            Active = 1
        }

        private sealed class PocoWithNullableEnum
        {
            public StatusEnum? Status
            {
                get; set;
            }
        }
    }
}
