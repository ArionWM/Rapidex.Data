# Entity Definition and Entity Types

Entities in the framework are defined through concrete classes or entity definition files (JSON or YAML). These definitions specify the structure, fields, relationships, and behaviors of the entities used within the application.

Each entity corresponds to one database table and includes fields that map directly to the columns of those tables. Fields can represent not only simple columns but also relationships to other entities — such as one-to-many, many-to-many, or other complex associations.

For better serialization performance and ease of use, entities themselves do not contain business logic. Each entity is DTO-like (Data Transfer Object) and is primarily focused on data representation.
Instead, they are associated with *implementer*s that encapsulate the corresponding logic.

## General Structure of an Entity Object

Each entity object typically includes the self metadata, scope (db or schema), fields dictionary. Even concrete entities behave like a dictionary.

## Concrete Entities

Concrete entities implement the IConcreteEntity interface. For basic usage, we provide the `DbConcreteEntityBase` class that can be extended to create concrete entities.

Properties starting with an underscore `_`` are not recognized as fields.

```csharp
public class Customer : DbConcreteEntityBase
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Email { get; set; }
}
```

## Soft Entities (JSON or YAML based)

Soft entities are defined using JSON or YAML files and can represent more flexible or dynamic data structures. They may not have a direct mapping to database tables but can be used to define entities that are more fluid in nature, such as user-generated content or external API responses.

### JSON

abc

```json
abc

```

abc: virtualize json?

### YAML

abc

```yaml
abc
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

See: [Library Declaration](LibraryDeclaration.md)

See: [Sample Application](/samples/)


### Applying Entity Definitions

Rapidex.Data is not apply entity definitions automatically during loading. During startup or runtime, after loading entity definitions, you need to apply them to the database metadata as shown below:

```csharp
db.Structure.ApplyAllStructure();
```

In runtime, you can change definitions and reapply them as needed.


## Partial Entities

Partial entities allow you to define a subset of an entity's fields and behaviors. This can be useful for scenarios where you want to work with a simplified version of an entity without loading the entire entity definition.

## Field Definitions

Fields contains name and type information and field specific properties.

```yaml
abc
```

## Predefined Fields

Each entity has several predefined fields that are commonly used across different entities. These fields include:

- `Id`: A unique identifier for the entity.
- `ExternalId`: An external identifier that can be used for integration with other systems.
- `DbVersion`: A version number for concurrency control.

## Predefined Properties (nonfield)

Each entity object has following predefined properties that are not considered fields:

`_Metadata`: Metadata reference about the entity.
`_virtualId`: Store premature id and planned future use.
`_Schema`: Db or schema scope reference.

## Nullable Fields

Nullable fields can be specified in entity definitions, allowing for fields that may not always have a value.

See: [Roadmap](Roadmap.md) 

## Implementer Infrastructure for Entity Logic

Classes

See: [Entity Logic](EntityLogic.md)
