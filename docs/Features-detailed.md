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

### Change metadata (entity definitions) and schema in runtime

Rapidex.Data allows to change entity definitions (metadata) and apply schema changes in runtime. 

- You can add a new field to an entity and apply the schema change in runtime without stopping the application.
- You can add *behavior* to an entity in runtime.

### UnitOfWork pattern

Rapidex.Data use the UnitOfWork pattern to group multiple operations into a single transaction. 

This ensures data integrity and consistency, especially in multithread applications.

```csharp
    var db = Database.Dbs.Db(); 

    using var work1 = db.BeginWork();

    //....

    work1.CommitChanges();
```


### Unique and extensible *behavior* approach for reusable logic

Rapidex.Data uses the [behavior](Behaviors.md) infrastructure to customize the definition and behavior of entities.

You can create reusable behaviors and attach them to multiple entities.
```csharp

```

...

### Dynamic logic injection with *signal hub (Rapidex Common)

Validate, calculate, before save, after save and other logic can be injected with *signal hub*.

Much more possibilities with *SignalHub* structure.

```csharp

internal class ContactImplementer : IConcreteEntityImplementer<Contact>
{
    protected static void CalculateContactValues(Contact contact)
    {
        if (contact.BirthDate.IsNOTNullOrEmpty())
        {
            DateTimeOffset now = DateTimeOffset.Now;
            int age = now.Year - contact.BirthDate.Value.Year;
            if (now.DayOfYear < contact.BirthDate.Value.DayOfYear)
                age--;
            contact.Age = age;
        }

        if (contact.FullName.IsNullOrEmpty())
        {
            contact.FullName = (contact.FirstName + " " + contact.LastName).Trim();
        }
    }

    protected static ISignalHandlingResult Validate(IEntityReleatedMessageArguments args)
    {
        Contact contact = (Contact)args.Entity.EnsureForActualEntity();
        IValidationResult validationResult = new ValidationResult();

        // ....

        if (contact.FirstName.IsNullOrEmpty())
            validationResult.Error("FirstName", "First name is required.");

        return args.CreateHandlingValidationResult(contact, validationResult);
    }

    protected static ISignalHandlingResult ExecLogic(IEntityReleatedMessageArguments args)
    {
        Contact contact = (Contact)args.Entity.EnsureForActualEntity();

        CalculateContactValues(contact);
        return args.CreateHandlingResult(contact);
    }

    protected static ISignalHandlingResult BeforeSave(IEntityReleatedMessageArguments args)
    {
        Contact contact = (Contact)args.Entity.EnsureForActualEntity();
        CalculateContactValues(contact);
        return args.CreateHandlingResult();
    }

    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        metadata
            .AddBehavior<ArchiveEntity>(true, false)
            .AddBehavior<HasTags>(true, false)
            .MarkOnlyBaseSchema();

        //See: SignalHub.md
        //Register to signals
        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_BEFORESAVE,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(Contact),
            ContactImplementer.BeforeSave);

        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_VALIDATE,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(Contact),
            ContactImplementer.Validate);

        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_EXEC_LOGIC,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(Contact),
            ContactImplementer.ExecLogic);
    }
}
```

...

### Web service ready for CRUD operations (with JSON)

...

### Easy predefined data support for entities

Much applications need predefined data for some entities (like Country, Currency, etc.). *Rapidex.Data* provides an easy way to define and apply this data with YAML format.

See: [Predefined Data](PredefinedData.md)

### PartialEntity structure supports selective data retrieval and updates

*PartialEntity* structure allows to defining a subset of fields to be retrieved or updated.
This is useful for optimizing performance and minimizing data transfer.

See: [Entity Definition](EntityDefinition.md#Partial%20Entities) and [Updating Data](UpdatingData.md#Partial%20Entities)

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

### Multiple database support in same application (multi tenant)

An application can use multiple databases (especially can use MultiTenant applications). 

```csharp
    var db = Database.Dbs.Db(); // default (master) database
    //....

    var otherDb = Database.Dbs.Db("otherDb");
    //....
```

See: [Multiple Database Management](MultiDatabaseManagement.md)

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

## Other Features

### Support for complex queries, filtering and pagination

...

See: [Querying Data](QueryingData.md)
See: [Filtering](Filtering.md)

### Built-in field types

Color, Currency, Email, Image, Json, Password, OneWayPassword Phone, Text, Time, and others.

And expandable!

See: [Field Types](FieldTypes.md)

### Easy use One-to-Many and Many-to-Many relationships

...


### Lazy Loading

...

### Automatic schema apply (create & update)

...

### Entity Ids always start from 10.000

To avoid conflicts with predefined data and demo data, entity Ids always start from 10.000.

Developers can inject predefined data and demo data with Ids less than 10.000.

### Unique and easy use *Query* object for complex queries  

abc


### Bulk update with queries

abc

### Text based filter parsing for dynamic filters

abc UI usage vs.

### Metadata injection from another entity definition

abc



