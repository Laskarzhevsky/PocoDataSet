# Code Review Checklist

## Ownership

- Does the change introduce a new reference from child to parent?
- Does a removed object still retain copied metadata from its former owner?
- Are JSON bridge dictionaries kept synchronized with runtime dictionaries?

## Row lifecycle

- Does the change preserve Added/Modified/Deleted/Detached semantics?
- Is physical removal performed at table level, not row level?
- Are original values preserved or cleared according to row state rules?

## Relations

- If a table is removed, are dependent relations cleaned?
- If relation validation changes, are both single-column and composite relations tested?

## Observable layer

- Does every event subscription have an unsubscribe path?
- Are views disposed when table/requestor is removed?
- Are event counts deterministic in merge tests?

## Serialization

- Does round-trip still preserve row kind and state?
- Are primitive object values still deserialized as primitives?
- Are interface converters still registered?

## Merge

- Is the desired merge mode explicit?
- Are local changes preserved or overwritten intentionally?
- Are server-generated values reconciled intentionally?
