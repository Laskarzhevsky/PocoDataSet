using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class DataRowExtensionsCopyToPocoMoreTests
    {
        [Fact]
        public void CopyToPoco_WhenRowContainsGuidValue_CopiesGuid()
        {
            // Arrange
            Guid g = Guid.NewGuid();

            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("ClientKey", DataTypeNames.GUID);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["ClientKey"] = g;

            PocoWithGuid poco = new PocoWithGuid();

            // Act
            row.CopyToPoco(poco);

            // Assert
            Assert.Equal(g, poco.ClientKey);
        }

        [Fact]
        public void CopyToPoco_WhenRowContainsGuidAsString_DoesNotParse_AndLeavesExistingValue()
        {
            // NOTE: CopyToPoco uses Convert.ChangeType and has no Guid special-case like ToPoco.
            // A string Guid will fail conversion and be swallowed.
            // Arrange
            Guid existing = Guid.NewGuid();
            Guid incoming = Guid.NewGuid();

            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("ClientKey", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["ClientKey"] = incoming.ToString();

            PocoWithGuid poco = new PocoWithGuid();
            poco.ClientKey = existing;

            // Act
            row.CopyToPoco(poco);

            // Assert (unchanged)
            Assert.Equal(existing, poco.ClientKey);
        }

        [Fact]
        public void CopyToPoco_WhenRowContainsEnumAsInt_ParsesNumeric()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Status", DataTypeNames.INT32);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Status"] = 2;

            PocoWithEnum poco = new PocoWithEnum();
            poco.Status = SampleStatus.Unknown;

            // Act
            row.CopyToPoco(poco);

            // Assert
            Assert.Equal(SampleStatus.Inactive, poco.Status);
        }

        [Fact]
        public void CopyToPoco_WhenTargetPropertyIsNullableEnum_DoesNotAssign_AndLeavesValue()
        {
            // NOTE: Nullable<TEnum> is not treated as Enum by CopyToPoco.
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("NullableStatus", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["NullableStatus"] = "Active";

            PocoWithNullableEnum poco = new PocoWithNullableEnum();
            poco.NullableStatus = SampleStatus.Inactive;

            // Act
            row.CopyToPoco(poco);

            // Assert (unchanged)
            Assert.Equal(SampleStatus.Inactive, poco.NullableStatus);
        }

        [Fact]
        public void CopyToPoco_CopiesBooleanAndDecimalAndDateTime_FromStronglyTypedValues()
        {
            // Arrange
            DateTime when = new DateTime(2026, 1, 3, 12, 34, 56, DateTimeKind.Utc);

            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("IsActive", DataTypeNames.BOOL);
            t.AddColumn("Amount", DataTypeNames.DECIMAL);
            t.AddColumn("WhenUtc", DataTypeNames.DATE_TIME);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["IsActive"] = true;
            row["Amount"] = 12.50m;
            row["WhenUtc"] = when;

            PocoWithMixedTypes poco = new PocoWithMixedTypes();

            // Act
            row.CopyToPoco(poco);

            // Assert
            Assert.True(poco.IsActive);
            Assert.Equal(12.50m, poco.Amount);
            Assert.Equal(when, poco.WhenUtc);
        }

        private enum SampleStatus
        {
            Unknown = 0,
            Active = 1,
            Inactive = 2
        }

        private sealed class PocoWithGuid
        {
            public Guid ClientKey { get; set; }
        }

        private sealed class PocoWithEnum
        {
            public SampleStatus Status { get; set; }
        }

        private sealed class PocoWithNullableEnum
        {
            public SampleStatus? NullableStatus { get; set; }
        }

        private sealed class PocoWithMixedTypes
        {
            public bool IsActive { get; set; }

            public decimal Amount { get; set; }

            public DateTime WhenUtc { get; set; }
        }
    }
}
