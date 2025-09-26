# Tips





## Use the IEntity interface even for concrete entities

...

Sample:

```csharp



```


## During deserialization, use IPartialEntity

When deserializing JSON data into entities, it's recommended to use the `IPartialEntity` interface. This approach provides flexibility and allows you to work with entities without needing to know their concrete types at compile time.

Because; 

- Json data may not contain all properties of the entity, using `IPartialEntity` helps to avoid issues related to missing properties. It allows you to work with the available data while still benefiting from the entity's metadata and structure.
- Json data my contain `delete` command or add / remove releation commands

> See: [Serialization and Deserialization of Entities](SerializationDeserializationJson.md)