# Schema and Primary Keys

## Schema owner

`DataTable` delegates schema ownership to internal `DataTableSchema`.

`DataTableSchema` owns:

```text
_columns
_columnsByName
_primaryKeyColumnNames
_clientKeyColumn
```

## Single source of truth for primary keys

Primary-key membership is defined by `IColumnMetadata.IsPrimaryKey`.

`PrimaryKeys` is a derived/cached list of column names rebuilt from column metadata flags.

This is an important architectural decision because older designs with a separate primary-key list can drift from column metadata.

## Default conventions

When a column is created without explicit metadata:

```text
Column named Id -> primary key by convention
Primary key -> not nullable
Non-primary column ending with Id -> foreign key by convention
Foreign key SomeNameId -> references table SomeName, column Id
```

## Client key

`__ClientKey` is a client-only correlation column.

Rules:

```text
Column name: __ClientKey
Data type: Guid
Nullable: false
Primary key: false
Purpose: correlate client-created rows with server post-save responses
```

## Row primary-key cache

Concrete `DataRow` keeps an internal list of primary-key column names copied from the table at attachment time.

This is **not** the authoritative primary-key definition. It is copied attachment metadata and must be cleared when the row is detached.

## Schema mutation warning

Changing primary keys after rows have already been attached is a high-risk operation because rows may have copied primary-key metadata. Any future public schema mutation API should either:

1. update attached rows, or
2. forbid primary-key changes after rows exist, or
3. clearly document that callers must rebuild/reattach rows.
