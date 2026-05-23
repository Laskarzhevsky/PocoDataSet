# Observable Lifecycle

## Purpose

The observable layer wraps core data objects to provide UI-friendly notifications without putting notification logic into the core data model.

## Main objects

```text
ObservableDataSet
ObservableDataTable
ObservableDataRow
ObservableDataView
```

## Event categories

Observable tables expose:

```text
CollectionChanged
PropertyChanged
RowsAdded
RowsRemoved
DataFieldValueChanged
RowStateChanged
```

Observable datasets expose table-level and propagated row/table events.

## Event-retention risk

The observable layer is the most important area to review for real .NET memory leaks because event subscriptions can keep objects alive.

## Current cleanup rules

`ObservableDataSet.RemoveObservableTable(tableName)` performs important cleanup:

```text
remove observable views pointing to the table
dispose those views
unsubscribe dataset handlers from table events
remove observable table from dictionary
remove inner table from inner dataset
```

`RemoveObservableDataView` and `RemoveObservableDataViewsForRequestor` dispose views through a key-based cleanup path.

## Maintainer rule

Any new observable object that subscribes to another observable object must have a symmetric cleanup path.

Required pattern:

```text
Subscribe during construction/addition.
Unsubscribe during removal/disposal.
Test that the remove path unsubscribes.
```

## Merge warning

Observable merges should avoid noisy event storms. The existing design includes observable-specific merge code rather than relying only on core merge code. This separation is intentional.
