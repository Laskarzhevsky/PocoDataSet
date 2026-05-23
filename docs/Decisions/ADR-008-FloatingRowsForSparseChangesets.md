# Floating rows for sparse changesets

Status: Accepted

## Decision

Use floating rows for Modified and Deleted changeset rows.

## Reason

Missing fields must mean "not provided", which is different from an explicit null value.

## Consequences

- Tests should lock this behavior.
- Public documentation should not promise behavior that contradicts this decision.
- Any future change should update this ADR or add a replacement ADR.
