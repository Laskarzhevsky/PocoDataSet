using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataTableExtensions
{
    public partial class ObservableDataTableExtensionsTests
    {
        [Fact]
        public void PrimaryKeys_WhenPrimaryKeyColumnsAdded_IsPopulatedOnObservableTable()
        {
            // Arrange
            IObservableDataSet observableDataSet = new ObservableDataSet();
            IObservableDataTable t = observableDataSet.AddNewTable("T");

            // Act
            t.AddColumn("Id", DataTypeNames.INT32, isNullable: false, isPrimaryKey: true);
            t.AddColumn("Code", DataTypeNames.STRING, isNullable: false, isPrimaryKey: true);
            t.AddColumn("Name", DataTypeNames.STRING);

            // Assert
            Assert.NotNull(t.PrimaryKeys);
            Assert.Equal(2, t.PrimaryKeys.Count);
            Assert.Equal("Id", t.PrimaryKeys[0]);
            Assert.Equal("Code", t.PrimaryKeys[1]);
        }

        [Fact]
        public void GetPrimaryKeyColumnNames_WhenNoOverride_ReturnsObservableTablePrimaryKeys()
        {
            // Arrange
            IObservableDataSet observableDataSet = new ObservableDataSet();
            IObservableDataTable t = observableDataSet.AddNewTable("T");

            t.AddColumn("Id", DataTypeNames.INT32, isNullable: false, isPrimaryKey: true);
            t.AddColumn("Name", DataTypeNames.STRING);

            // Act
            List<string> primaryKeyColumnNames = t.GetPrimaryKeyColumnNames(null);

            // Assert
            Assert.NotNull(primaryKeyColumnNames);
            Assert.Single(primaryKeyColumnNames);
            Assert.Equal("Id", primaryKeyColumnNames[0]);
        }

        [Fact]
        public void GetPrimaryKeyColumnNames_WhenOverrideProvided_ReturnsOverride()
        {
            // Arrange
            IObservableDataSet observableDataSet = new ObservableDataSet();
            IObservableDataTable t = observableDataSet.AddNewTable("T");

            t.AddColumn("Id", DataTypeNames.INT32, isNullable: false, isPrimaryKey: true);
            t.AddColumn("Name", DataTypeNames.STRING);

            ObservableMergeOptions options = new ObservableMergeOptions();
            options.OverriddenPrimaryKeyNames["T"] = new List<string> { "Name" };

            // Act
            List<string> primaryKeyColumnNames = options.GetPrimaryKeyColumnNames(t);

// Assert
            Assert.NotNull(primaryKeyColumnNames);
            Assert.Single(primaryKeyColumnNames);
            Assert.Equal("Name", primaryKeyColumnNames[0]);
        }
    }
}
