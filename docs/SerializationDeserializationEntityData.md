# Serialization and Deserialization of Entity Data

Rapidex Entities has custom serialization and deserialization logic to;

- Support data transfer for CRUD operations.
- Handle complex property types such as `Currency`, `Text`, `Image`, and `Enumeration<T>`. This ensures that these properties are correctly converted to and from JSON format.
- Can handle `new` and `delete` operations on entities, ensuring that the entity state is accurately represented in JSON.

## Serialization

To serialize an entity to JSON, you can use the `EntityDataJsonConverter.abc` or `ToJson` method or use `System.Text.Json.JsonSerializer.Serialize` methods

abc (default options usage?)

> If use `JsonSerializer.Serialize` methods, prefer using `JsonHelper.JsonSerializerOptions` for consistent serialization settings.

### Sample 1

Simple serialization `ConcreteEntity01` entity (See: Rapidex.UnitTest.Data):

```csharp

ConcreteEntity01 ent = db.New<ConcreteEntity01>();
ent.Name = "ent01_01";
ent.Phone = "555-1234";
ent.Number = 123;

string json = ent.ToJson(); //See below;

//or

string json = EntityDataJsonConverter.Serialize(ent);


```


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

To de-serialize an entity to JSON, you should use the `EntityDataJsonConverter.Deserialize` method.

```csharp

string json = "[...]";

var db = Database.Dbs.Db();

var entities = EntityDataJsonConverter.Deserialize(json, db);

```

Map to given concrete type

```csharp

string json = "[...]";

var db = Database.Dbs.Db();

ConcreteEntity01[] ents = EntityDataJsonConverter.Deserialize<ConcreteEntity01>(json, db);

```

> JSON properties are case-**insensitive** during deserialization. 

> You can use either `entity` or `_entity` for entity name and `id` or `_id` for entity identifier.



## Operations (Update, New, Delete, Add or Remove Related Entities)

For operational JSON; `Type` property are used to represent these operations. Type property contained json deserialization always create `PartialEntity`.

This entities use with `UnitOfWork` pattern for batch processing of multiple operations.

> Because; For concrete or full entities, partial data leads to override unspecified fields with their default values.

`type` property can have the following values; `update`, `new` and `delete`. 

> You can use either `type` or `_type` for operation type.

### Update Data

Update data can contains partial or full data for an entity. JSON can contains only the fields that need to be updated.

```Json
[
  {
    "entity": "MyConcreteEntity",
    "type": "update",
    "id": 10041,
    "Values": {
      "ConcreteField": "555-5678",
      "AnotherConcreteField": 456
    }
  },
  {
    "_entity": "MySoftEntity",
    "type": "update",
    "_id": 10042,
    "Values": {
      "Field": "Updated Name",
      "AnotherField": 123
    }
  }
]
```

```csharp

string json = "[{ "type": "update", ... } ]";

var db = Database.Dbs.Db();

var entities = EntityDataJsonConverter.Deserialize(json, db);

using var uow = db.BeginWork();
entities.Save(uow);
uow.CommitChanges();

```

### New Entity

New entity JSON representation can include only the fields that need to be set. 

```Json
[
  {
    "entity": "MyConcreteEntity",
    "type": "new",
    "Values": {
      "ConcreteField": "555-5678",
      "AnotherConcreteField": 456
    }
  },
  {
    "entity": "MySoftEntity",
    "type": "new",
    "Values": {
      "Field": "New Name",
      "AnotherField": 123
    }
  }
]
```

#### Specify Id for New Entity

... abc

### Delete Entity

```Json
[
  {
    "entity": "MyConcreteEntity",
    "type": "delete",
    "id": 10041
  },
  {
    "entity": "MySoftEntity",
    "type": "delete",
    "id": 10042
  }
]
```



### Add or Remove Related Entities

...



## Remarks

- `values` node properties is case-insensitive during deserialization.


## How To Work

Rapidex using `System.Text.Json` for JSON serialization and deserialization. 
For `System.Text.Json` simple usage, we provide `EntityDataJsonConverter` class.`

