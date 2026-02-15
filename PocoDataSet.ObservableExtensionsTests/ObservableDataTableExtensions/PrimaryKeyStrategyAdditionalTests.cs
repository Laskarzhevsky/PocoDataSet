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
        public void PrimaryKeys_CompositeKeyOrder_IsPreserved()
        {
            // Arrange
            IObservableDataSet observableDataSet = new ObservableDataSet();
            IObservableDataTable t = observableDataSet.AddNewTable("T");

            // Act
            t.AddColumn("Id", DataTypeNames.INT32, isNullable: false, isPrimaryKey: true);
            t.AddColumn("Code", DataTypeNames.STRING, isNullable: false, isPrimaryKey: true);
            t.AddColumn("Name", DataTypeNames.STRING);

            // Assert
            Assert.Equal(2, t.PrimaryKeys.Count);
            Assert.Equal("Id", t.PrimaryKeys[0]);
            Assert.Equal("Code", t.PrimaryKeys[1]);
        }

        [Fact]
        public void GetPrimaryKeyColumnNames_WhenOverrideExists_OverrideWinsOverSchema()
        {
            // Arrange
            IObservableDataSet observableDataSet = new ObservableDataSet();
            IObservableDataTable t = observableDataSet.AddNewTable("T");

            t.AddColumn("Id", DataTypeNames.INT32, isNullable: false, isPrimaryKey: true);
            t.AddColumn("Name", DataTypeNames.STRING);

            ObservableMergeOptions options = new ObservableMergeOptions();
            options.OverriddenPrimaryKeyNames["T"] = new List<string> { "Name" };

            // Act
            List<string> primaryKeyColumnNames = options.GetPrimaryKeyColumnNames(t.InnerDataTable);

            // Assert
            Assert.Single(primaryKeyColumnNames);
            Assert.Equal("Name", primaryKeyColumnNames[0]);
        }
    }
}
