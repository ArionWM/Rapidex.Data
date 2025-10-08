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

## Bulk Update

abc

## JSON Deserialization for CRUD Operations


Rapidex JSON deserialization infrastructure supports `new`, `update`, `delete` and add/remove relations operations on entities.

See: [Serialization and Deserialization of Entities](/docs/SerializationDeserializationEntityData.md)