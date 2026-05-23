# Interface Projections

## Purpose

Interface projection allows a dynamic row to be viewed through a strongly typed interface. This avoids creating many DTO classes for every projected shape.

## Public value

The website documentation presents this as a major POCO DataSet feature: the same physical row/table can support different interface views depending on caller needs.

## Main operations

```text
DataRowExtensions.AsInterface<TInterface>()
DataSetExtensions.AsInterface<TInterface>(tableName, rowIndex)
DataTableExtensions.AsInterface<TInterface>(rowIndex)
DataSetExtensions.AddNewTableFromPocoInterface<TInterface>()
DataTableExtensions.AddColumnsFromInterface<TInterface>()
```

## Maintainer rule

Interface projections should remain an overlay on top of row values. They should not require concrete row subclasses or code generation.

## Naming convention warning

The project has discussed whether to remove a leading `I` from interface names when creating table names. If this behavior is changed, it should be documented as a compatibility decision because it affects generated table names and examples.
