# Serializer Contract

## Entry points

```text
DataSetSerializer.ToJsonString(IDataSet?)
DataSetSerializer.FromJsonString(string?)
```

## Null behavior

```text
ToJsonString(null) -> null
FromJsonString(null or empty) -> null
```

## Required converters

The serializer must register converters for interface-to-concrete mapping and row polymorphism.

## Value behavior

Object values should deserialize to primitive .NET values where possible, not `JsonElement` objects.

## Normalization

Deserialized datasets must be normalized after reading.
