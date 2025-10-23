# Entity Definition and Entity Types

Entities in the Rapidex.Data are defined through concrete classes or entity definition files (JSON or YAML). These definitions specify the structure, fields, relationships, and behaviors of the entities used within the application.

Each entity corresponds to one database table and includes fields that map directly to the columns of those tables. Fields can represent not only simple columns but also relationships to other entities — such as one-to-many, many-to-many, or other complex associations.

For better serialization performance and ease of use, entities themselves do not contain business logic. Each entity is DTO-like (Data Transfer Object) and is primarily focused on data representation.
Instead, they are associated with *implementer*s that encapsulate the corresponding logic.

Each entity object typically includes the self metadata, scope (db or schema), fields dictionary. Even concrete entities behave like a dictionary.

## Concrete Entities

Concrete entities implement the IConcreteEntity interface. For basic usage, we provide the `DbConcreteEntityBase` class that can be extended to create concrete entities.

Properties starting with an underscore `_` are not recognized as fields.

```csharp
public class MyEntity : DbConcreteEntityBase
{
    public string Subject { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeStartEnd PlannedTimes { get; set; }
    public Tags Tags { get; set; }
    public Reference<Customer> Customer { get; set; }
    public Enumeration<MyEntityTypeEnum> Type { get; set; }
}

public enum MyEntityTypeEnum
{
    Type1 = 1,
    Type2 = 2,
    Type3 = 3
}

public class MyEntityImplementer : IConcreteEntityImplementer<MyEntity>
{
    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        metadata
            .AddBehavior<HasTags>(true, false);
    }
}
```

## Soft Entities (JSON or YAML based)

Soft entities are defined using JSON or YAML files and can represent more end-user friendly environments.

### JSON

```json
{
  "type": "EntityDefinition",
  "name": "MyEntity",
  "fields": [
    {
      "name": "Subject",
      "type": "string"
    },
    {
      "name": "StartTime",
      "type": "datetime"
    },
    {
      "name": "PlannedTimes",
      "type": "dateTimeStartEnd"
    },
    {
      "name": "Customer",
      "type": "reference",
      "reference": "Customer"
    },
    {
      "name": "Type",
      "type": "enumeration",
      "reference": "MyEntityTypeEnum"
    }
  ],
  "behaviors": [
    "HasTags"
  ]
}

```

### YAML

```yaml
_tag: entity
name: MyEntity
fields:
  - name: Subject
    type: string
  - name: StartTime
    type: datetime
  - name: PlannedTimes
    type: dateTimeStartEnd
  - name: Customer
    type: reference
    reference: Customer
  - name: Type
    type: enumeration
    reference: MyEntityTypeEnum
behaviors: [hasTags]  
```

## Metadata

*Metadata* objects contain information about the entity definition and predefined data. Each database has own metadata management and use different definitions (and/or for same entity name) in different databases.

This approach provides advanced customization and flexibility, as each database can manage entity definitions according to its own specific requirements.

## Loading Entity Definitions

Entity definitions metadata can be loaded with automatic scanning or manually from specific approaches.

### Concrete Entities

For automatic scanning, concrete entities are discovered via reflection;

If the library containing the entity definitions includes a Library Declaration, it is scanned during the Rapidex.Data Startup and the definitions are loaded.

If the library is not already included, you can add it manually during setup as shown below:

```csharp
Common.Assembly.Add(typeof(Program).Assembly);
```

### Soft Entities (YAML or JSON based)

If the library containing the entity definitions as files a Library Declaration, Rapidex.Data will scan the `app_content` + *library name* folder during startup and load the definitions. 

Default `app_content` folder name definition can be overridden via configuration. 

For manual loading, you can specify the folder path where the entity definition files are located:

```csharp
var db = Database.Dbs.AddMainDbIfNotExists();
db.Metadata.ScanDefinitions(@".\MyDefinitionRoot\MyAppDefinitions");
```


#### Field Definitions In Soft Entities

Fields contains name and type information and field specific properties.


### Applying Entity Definitions

Rapidex.Data is not apply entity definitions automatically during loading. During startup or runtime, after loading entity definitions, you need to apply them to the database metadata as shown below:

```csharp
db.Structure.ApplyAllStructure();
```

In runtime, you can change definitions and reapply them as needed.

## Partial Entities

Partial entities allow you to define a subset of an entity's fields and behaviors. This can be useful for scenarios where you want to work with a simplified version of an entity without loading the entire entity definition.

```csharp
var db = Database.Dbs.Db(); //<- Master db

ILoadResult<DataRow> partialData = db.GetQuery<Contact>()
    .Like(nameof(Contact.FullName), "John")
    .LoadPartial(nameof(Contact.FirstName), nameof(Contact.LastName));

//...
```

Also partial entities used in [JSON deserialization](SerializationDeserializationEntityData.md).

## Predefined Fields

Each entity has several predefined fields that are commonly used across different entities. These fields include:

- `Id`: A unique identifier for the entity.
- `ExternalId`: An external identifier that can be used for integration with other systems.
- `DbVersion`: A version number for concurrency control.

## Predefined Properties (nonfield)

Each entity object has following predefined properties that are not considered fields:

- `_Metadata`: Metadata reference about the entity.
- `_virtualId`: Store premature id and planned future use.
- `_Schema`: Db or schema scope reference.

## Nullable Fields

Nullable fields can be specified in entity definitions, but they convert to non-nullable types in concrete entities. 

*Future improvements are planned to better support nullable fields in concrete entities. See: [Roadmap](Roadmap.md)*

## Implementer Infrastructure for Entity Logic

Entity logic, including validation and custom calculations, is managed through the Implementer infrastructure. Each entity can have an associated implementer class that encapsulates the logic specific to that entity.

## See Also

- See: [Quick Start](QuickStart.md)
- See: [Library Declaration](LibraryDeclaration.md)
- See: [Sample Application](/samples/)
- See: [Field Types](FieldTypes.md)
- See: [Entity Logic](EntityLogic.md)
- See: [Serialization and Deserialization of Entities](SerializationDeserializationEntityData.md)
- See: [Usage and Tips](UsageAndTips.md)
