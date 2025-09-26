# Serialization and Deserialization of Entities

Rapidex Entities has custom serialization and deserialization logic to;

- Support data transfer for CRUD operations.
- Handle complex property types such as `Currency`, `Text`, `Image`, and `Enumeration<T>`. This ensures that these properties are correctly converted to and from JSON format.
- Can handle `new` and `delete` operations on entities, ensuring that the entity state is accurately represented in JSON.

## Serialization

To serialize an entity to JSON, you can use the `ToJson` method or use `System.Text.Json.JsonSerializer.Serialize` methods

> If use `JsonSerializer.Serialize` methods, prefer using `JsonHelper.JsonSerializerOptions` for consistent serialization settings.

### Sample 1

Simple serialization result for `ConcreteEntity01` entity (See: Rapidex.UnitTest.Data):

```json
[
  {
    "_entity": "ConcreteEntity01",
    "_caption": "ent01_01",
    "_id": 10041,
    "Id": 10041,
    "DbVersion": 0,
    "Values": {
      "Name": "ent01_01",
      "Address": null,
      "Phone": "555-1234",
      "Number": 123,
      "CreditLimit1": 10000,
      "CreditLimit1Currency": "USD",
      "CreditLimit2": 0,
      "CreditLimit2Currency": null,
      "Total": 10000,
      "TotalCurrency": null,
      "Description": {
        "value": "Description for ent01",
        "type": "Plain"
      },
      "DescriptionType": null,
      "Picture": {
        "value": 10006,
        "text": "Image01.png",
        "Id": "Base.ConcreteEntity01.10041.fields.Picture",
        "_id": "Base.ConcreteEntity01.10041.fields.Picture"
      },
      "BirthDate": "1990-01-01T00:00:00+00:00",
      "ContactType": {
        "value": 16,
        "text": "Personal"
      },
      "Id": 10041,
      "ExternalId": null,
      "DbVersion": 0
    }
  }
]
```

- Entity metadata locate properties prefixed with `_` (underscore) at the root level of the JSON object.
- Field values are grouped under the `Values` property.
- Complex types such as `Currency`, `Text`, `Image`, and `Enumeration<T>` are serialized into structured JSON objects to preserve their data. This structures generally has `value`, `text` or `type` properties. Field value, metadata and useful information are included in the serialized output. See: [Field types](FieldTypes.md)
- Serialization result has not include db or schema specific information.


## Deserialization

```csharp

string json = "[...]";

var db = Database.Dbs.Db();

var entities = EntityJson.Deserialize(json, db);

```

Map to given concrete type

```csharp

string json = "[...]";

var db = Database.Dbs.Db();

ConcreteEntity01[] ents = EntityJson.Deserialize<ConcreteEntity01>(json, db);

```




## Operations

### New and Delete Operations

JSON representation of entities can include operation information to indicate whether the entity is new or should be deleted. 

`IsNew` and `IsDeleted` properties are used to represent these operations.

- `IsNew`: Indicates that the entity is new and should be created in the database.
- `IsDeleted`: Indicates that the entity should be deleted from the database.

```Json

abc

```


### Add or Remove Related Entities

...



## Remarks

- `values` node properties is case-insensitive during deserialization.

