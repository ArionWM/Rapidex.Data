# Updating Data

.....

## UnitOfWork Pattern

```csharp
using (var uow = db.BeginWork())
{
    abc

    //...

    uow.Commit();
}
```


## Partial Entities

abc

## Json Deserialization for CRUD Operations


Rapidex Json deserialization infrastructure supports `new`, `update` and `delete` operations on entities.


See: [Serialization and Deserialization of Entities](/docs/SerializationDeserializationJson.md)