using System.Collections.Generic;

using Xunit;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class DataTablePrimaryKeysPropagationTests
    {
        [Fact]
        public void AddColumn_WhenColumnIsPrimaryKey_PopulatesTablePrimaryKeys()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");

            // Act: this overload infers primary key for "Id" column in your library
            t.AddColumn("Id", DataTypeNames.INT32);

            // Assert
            Assert.NotNull(t.PrimaryKeys);
            Assert.Contains("Id", t.PrimaryKeys);
        }

        [Fact]
        public void AddColumns_WhenPrimaryKeysNotProvided_RebuildsFromColumnMetadata()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");

            List<IColumnMetadata> cols = new List<IColumnMetadata>();

            ColumnMetadata id = new ColumnMetadata();
            id.ColumnName = "Id";
            id.DataType = DataTypeNames.INT32;
            id.IsNullable = false;
            id.IsPrimaryKey = true;
            cols.Add(id);

            ColumnMetadata name = new ColumnMetadata();
            name.ColumnName = "Name";
            name.DataType = DataTypeNames.STRING;
            name.IsNullable = true;
            name.IsPrimaryKey = false;
            cols.Add(name);

            // Act
            t.AddColumns(cols);

            // Assert
            Assert.NotNull(t.PrimaryKeys);
            Assert.Single(t.PrimaryKeys);
            Assert.Equal("Id", t.PrimaryKeys[0]);
        }

        [Fact]
        public void AddColumn_WhenCalledTwice_DoesNotDuplicatePrimaryKey()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");

            // Act
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Id2", DataTypeNames.INT32, isNullable: false, isPrimaryKey: true, isForeignKey: false);

            // Assert: should contain Id and Id2 only once each
            int idCount = 0;
            int id2Count = 0;

            for (int i = 0; i < t.PrimaryKeys.Count; i++)
            {
                if (t.PrimaryKeys[i] == "Id")
                {
                    idCount++;
                }

                if (t.PrimaryKeys[i] == "Id2")
                {
                    id2Count++;
                }
            }

            Assert.Equal(1, idCount);
            Assert.Equal(1, id2Count);
        }
    }
}
