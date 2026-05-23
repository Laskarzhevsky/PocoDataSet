# Relation Contract

## Storage

Relations store table and column names only.

## No object references

A relation must not store object references to DataSet, DataTable, DataRow, or ColumnMetadata.

## Integrity

Core relation storage is lightweight. Full relation validation is performed by extensions.

## Table removal

Removing a table must remove all relations where the table is parent or child.

## Composite keys

Parent and child column-name lists must have the same count. This allows composite relations.
