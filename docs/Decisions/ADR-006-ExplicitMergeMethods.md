# Explicit merge methods replace generic MergeWith

Status: Accepted

## Decision

Use `DoPostSaveMerge`, `DoRefreshMergeIfNoChangesExist`, `DoRefreshMergePreservingLocalChanges`, and `DoReplaceMerge`.

## Reason

One generic `MergeWith` made semantics harder to reason about. Explicit methods make intent and tests clearer.

## Consequences

- Tests should lock this behavior.
- Public documentation should not promise behavior that contradicts this decision.
- Any future change should update this ADR or add a replacement ADR.
