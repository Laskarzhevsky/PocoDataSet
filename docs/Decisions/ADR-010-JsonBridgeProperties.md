# JSON bridge properties

Status: Accepted

## Decision

Use concrete JSON bridge properties such as `TablesJson`, `RowsJson`, `ValuesJson`, and `OriginalValuesJson`.

## Reason

System.Text.Json cannot directly populate the read-only interface-based public API. Bridge properties preserve the public contracts while enabling round-trips.

## Consequences

- Tests should lock this behavior.
- Public documentation should not promise behavior that contradicts this decision.
- Any future change should update this ADR or add a replacement ADR.
