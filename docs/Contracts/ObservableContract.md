# Observable Contract

## Purpose

Observable wrappers provide UI-friendly notifications without polluting the core model.

## Event cleanup

Every event subscription must have a cleanup path.

## Table removal

Removing an observable table must:

```text
remove related observable views
dispose/unsubscribe those views
unsubscribe dataset handlers from table events
remove observable table from observable dictionary
remove inner table from inner dataset
```

## View removal

Removing a view must dispose/unsubscribe it and remove it from the view cache.

## Merge behavior

Observable merge operations should preserve correctness while avoiding unnecessary event noise.
