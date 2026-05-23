# Primary key source of truth

Status: Accepted

## Decision

`ColumnMetadata.IsPrimaryKey` is the authoritative source of primary-key membership.

## Reason

A separate mutable primary-key list can drift from column metadata. The current design derives/caches primary-key names from column flags.

## Consequences

- Tests should lock this behavior.
- Public documentation should not promise behavior that contradicts this decision.
- Any future change should update this ADR or add a replacement ADR.
