# Rapidex Features

## Simple and flexible 

For easy use and fast development. But also flexible for complex scenarios.

## Main Features

### Define entities with *concrete* (classes) or *soft* (JSON, YAML or with code in runtime)

For ORMs, only concrete classes or static mapping (XML etc.) definitions has flexibility limitations.

Rapidex.Data allows to define entities with concrete classes or with JSON, YAML files. 

Also entity definitions can be created with code in runtime.

```csharp
public class Product : DbConcreteEntityBase
{
	public string Name { get; set; }
	public decimal Price { get; set; }
}
```

thats all! No additional attributes or configuration required.

Or with JSON:
```json
{
  "type": "EntityDefinition",
  "name": "Product",
  "fields": [
    {
      "name": "Id",
      "type": "long",
    },
    {
      "name": "Subject",
      "type": "string"
    },
    {
      "name": "Customer",
      "type": "reference",
      "reference": "Contact"
    }

    //...
  ]
}
```

Or with YAML:
```yaml
_tag: entity
name: Product
fields:
  - name: Id
    type: long
  - name: Subject
    type: string
  - name: Customer
    type: reference
    reference: Contact

    #...
```

---

### Change metadata (entity definitions) and schema in runtime

Rapidex.Data allows to change entity definitions (metadata) and apply schema changes in runtime. 

- You can add a new field to an entity and apply the schema change in runtime without stopping the application.
- You can add *behavior* to an entity in runtime.

---

### UnitOfWork pattern

Rapidex.Data use the UnitOfWork pattern to group multiple operations into a single transaction. 

This ensures data integrity and consistency, especially in multithread applications.

```csharp
    var db = Database.Dbs.Db(); 

    using var work1 = db.BeginWork();

    //....

    work1.CommitChanges();
```

---

### Unique and extensible *behavior* approach for reusable logic

Rapidex.Data uses the [behavior](Behaviors.md) infrastructure to customize the definition and behavior of entities.

You can create reusable behaviors and attach them to multiple entities.

---

### Dynamic logic injection with *signal hub*

Validate, calculate, before save, after save and other logic can be injected with *signal hub*.

Much more possibilities with *SignalHub* structure.

See: [Entity Logic](EntityLogic.md)

See: [Signal Hub](SignalHub.md)

---

### Web service ready for CRUD operations (with JSON)

Rapidex.Data ready for CRUD operations + web services implementation and provides built-in JSON deserialization support. 

See: [Serialization & Deserialization](SerializationDeserializationEntityData.md)

See: [Sample ASP.NET Core Application](/samples/Rapidex.Data.Sample.Basic1)

---

### Easy predefined data support for entities

Much applications need predefined data for some entities (like Country, Currency, etc.). *Rapidex.Data* provides an easy way to define and apply this data with YAML format.

See: [Predefined Data](PredefinedData.md)

---

### PartialEntity structure supports selective data retrieval and updates

*PartialEntity* structure allows to defining a subset of fields to be retrieved or updated.
This is useful for optimizing performance and minimizing data transfer.

See: [Entity Definition](EntityDefinition.md#Partial-Entities) 

See: [Updating Data](UpdatingData.md#Partial-Entities)

---

### Multiple schema (workspaces) support

Some database engines (like PostgreSQL or MS SQL) support multiple schemas in a single database. 

Rapidex.Data can use multiple schemas for workspace isolation. Each schema (workspace) can have its own set of tables (entities). 

Some entities can be marked *only base schema* and shared between all schemas (workspaces).

```csharp
    var baseSchema = Database.Dbs.Db(); // default schema (name: 'base')
    using var work = baseSchema.BeginWork();
    //....

    var schema1 = baseSchema.Schema("myOtherSchema"); // another schema (or workspace)
    using var work = schema1.BeginWork();
    //....
```

See: [Multi Schema Management](MultiSchemaManagement.md)

---

### Multiple database support in same application (multi tenant)

An application can use multiple databases (especially can use MultiTenant applications). 

```csharp
    var db = Database.Dbs.Db(); // default (master) database
    //....

    var otherDb = Database.Dbs.Db("otherDb");
    //....
```

See: [Multiple Database Management](MultiDatabaseManagement.md)

---

### Different metadata customization for each database connection (each tenant can have different configuration)

Each database can have its own set of entities (tables) and may have **different** metadata definitions.

```csharp
    var db = Database.Dbs.Db(); // default (master) database

    //....

    var otherDb = Database.Dbs.Db("otherDb");
    otherDb.Metadata.ScanDefinitions("..."); // scan entity definitions on folder (JSON, YAML files) for this database only
    otherDb.Metadata.AddJson("...") // And/or add / modify entity definitions with another way
    otherDb.Structure.ApplyAllStructure(); //Thats all! Apply schema changes for this database only

    //....
```

See: [Multiple Database Management](MultiDatabaseManagement.md)

---

## Other Features

### Support for complex queries, filtering and pagination

Rapidex.Data has built-in support for complex queries and pagination.

See: [Querying Data](QueryingData.md)

Also supports filtering (from string expressions) with various operators.

See: [Filtering](Filtering.md)

---

### Built-in field types

Color, Currency, Email, Image, Json, Password, OneWayPassword Phone, Text, Time, and others.

And expandable!

See: [Field Types](FieldTypes.md)

---

### Easy use One-to-Many and Many-to-Many relationships

Relationship support is designed with "field" infrastructure for easy development and end user experience.

`RelationN2N` and `RelationOne2N` field types are provide easy use of Many-to-Many and One-to-Many relationship support.

And this relationships is `lazy` and can be used in queries, filters, json serialization / deserialization etc.

See: [Field Types](FieldTypes.md)

---

### Lazy Loading

Reference, Relation, Enumeration and other complex field types are lazy loaded.

---

### Automatic schema apply (create & update)

Rapidex.Data can automatically create and update database schema based on entity definitions with single command.

```csharp
    var db = Database.Dbs.Db(); 
    //... scan definitions or add definitions with code
    db.Structure.ApplyAllStructure(); //<-- Apply structure changes
```

or 

```csharp
    var db = Database.Dbs.Db(); 
    
    var em = dbScope.Metadata.Get<Contact>();
    em.AddFieldIfNotExist<string>("DynamicAddedField");
    dbScope.Structure.ApplyEntityStructure<Contact>(); //<-- Apply structure changes for entity only
```

---

### Entity Ids always start from 10.000

To avoid conflicts with predefined data and demo data, entity Ids always start from 10.000.

Developers can inject predefined data and demo data with Ids less than 10.000.

---

### Unique and easy use *Query* object for complex queries  

`Query` object provides a unique and easy way to create complex queries with filtering, sorting, pagination and includes.

```csharp
    var db = Database.Dbs.Db(); 
    var query = db.GetQuery<Contact>()
            .Like(nameof(Contact.FullName), "%a%")
            .OrderBy(OrderDirection.Asc, nameof(Contact.FullName))
            .IsNotArchived();

    var result = query.Load();
```

---

### Bulk update with queries

`Query` object also supports bulk update operations, allowing you to update multiple records in a single command.

```csharp
    var work = db.BeginWork();
    var query = db.GetQuery<Contact>();
    
    query
        .EnterUpdateMode()
        .Eq(nameof(Contact.Type), ContactTypeSample.Personal)
        .Update(work, new Dictionary<string, object>() { { nameof(Contact.FullName), "Updated Name" } });

    work.CommitChanges();
```

---

### Text based filter parsing for dynamic filters

For web services and dynamic queries, Rapidex.Data supports text-based filter parsing.

See: [Filtering](Filtering.md)

---

### Metadata injection from another entity definition

Rapidex.Data supports metadata injection from another entity definition, allowing you to easily reuse and extend existing entity definitions.

```YAML
_tag: entity
name: myEntity5
tenant: common

fields:
- name: Type
  type: enum
  caption: 
  isSealed: false
  reference: ContactType

injection: # <- Inject to another entity definition 
- entityName: myOtherEntity
  fields:
  - name: InjectedField
    type: string
    caption: InjectedField

```

