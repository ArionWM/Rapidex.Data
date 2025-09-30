# Serialization and Deserialization of Entity Definitions (Metadata)

Entity Metadata can define with JSON and YAML formats and include entity field, behavior and predefined data definitions.


```yaml

abc

```

```json

abc

```


> See: abc


## Deserialization

Unlike data de-serialization, metadata de-serialization uses `implementer` classes for metadata construction.

**For JSON;**

```csharp

string content = "..."; // JSON content

var db = Database.Dbs.Db();

db.Metadata.AddJson(content);

```

**For YAML;**

```csharp

string content = "..."; // YAML content

var db = Database.Dbs.Db();

db.Metadata.AddYaml(content);

```

abc






## How To Work

**For JSON;**

Rapidex using `System.Text.Json` for JSON serialization and deserialization. 
For `System.Text.Json` limitations (discriminators etc), abc

`Impementer`s is not direct (independed) create and *return* metadata instances, but use incremental construction approach 
and add deserialized information to metadata definition on defined database scope.

For this approach, Rapidex uses `IEntityMetadataImplementer` service and several *implementer* classes for metadata construction.

**For YAML**

Rapidex convert YAML to JSON and use JSON deserialization approach.

