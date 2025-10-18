# Rapidex.Data

## What is Rapidex.Data?

**Rapidex.Data** is an **Object-Relational Mapping (ORM)** framework focused on *flexibility*, *ease of use*, and *runtime customization* for modern applications.

It allows entities to be defined using concrete classes, **JSON**, or **YAML** definition files — or even programmatically at runtime.  
Entity definitions (metadata) can also be modified and applied dynamically while the application is running.

It supports multiple database engines, including **Microsoft SQL Server**, **PostgreSQL**, and others (planned).  
Multiple database connections can be used within a single application instance.

**Rapidex.Data** is designed to support **multi-module**, **multi-workspace**, and **multi-tenant** architectures.

## Main Features

- Define entities with **concrete** (classes) or **soft** (JSON, YAML or with code in runtime) 
- Change metadata (entity definitions) and schema in runtime
- UnitOfWork pattern
- Unique and extensible *behavior* approach for reusable logic
- Dynamic logic injection with *signal hub (with Rapidex Common)* 
- Web service ready for CRUD operations (with JSON)
- Easy predefined data support for entities
- *PartialEntity* structure supports selective data retrieval and updates
- Multiple database support in same application (multi tenant)
- Multiple schema (workspaces) support
- Different metadata customization for each database connection (each tenant can have different configuration)

See: [Features](/docs/Features-detailed.md) for more details.

![License](https://img.shields.io/badge/license-%20%20GNU%20GPLv3%20-green)

## Status

![Status](https://img.shields.io/badge/status-early--development-%23f39c12)
![Stability](https://img.shields.io/badge/stability-mostly--stable-%234CAF50)
![Usability](https://img.shields.io/badge/usability-inhouse-blue)

Rapidex.Data is under active development, but its **core architecture and interfaces are now stable**.  
No major changes are expected in the framework’s structure or public API surface.  
Ongoing work focuses on completing feature sets, improving performance, and enhancing documentation.

> **In short:** The architecture is stable — development continues with focus on refinement rather than redesign.

## Packages (nuget)

| Package | Version |
|---|---|
|[Rapidex.Data](https://www.nuget.org/packages/abc/)|![JobMan](https://img.shields.io/nuget/v/abc)|
|[Rapidex.Data.SqlServer](https://www.nuget.org/packages/abc/)|![JobMan](https://img.shields.io/nuget/v/abc)|
|[Rapidex.Data.PostgreServer](https://www.nuget.org/packages/abc/)|![JobMan](https://img.shields.io/nuget/v/abc)|

---

## Quick Start

See: [Quick Start](/docs/QuickStart.md) 

## Samples

You can download and run sample project from the following links:

- [Sample Application](/samples/)

## Documentation

### Basics

- [Entity Definition](/docs/EntityDefinition.md)

- [Field Types](/docs/FieldTypes.md)

- [Entity Logic](/docs/EntityLogic.md)

- [Behaviors](/docs/Behaviors.md)

- [Querying Data](/docs/QueryingData.md)
  - [Filtering](/docs/Filtering.md)

- [Predefined Data](/docs/PredefinedData.md)

- [Database Connection and Required Rights](/docs/DatabaseConnectionAndRequiredRights.md)

- Serialization and Deserialization
  - [Data Serialization & Deserialization](/docs/SerializationDeserializationEntityData.md)
  - [Metadata Serialization](/docs/SerializationDeserializationMetadata.md)

- [Updating Data](/docs/UpdatingData.md)

- [Library Declaration](/docs/LibraryDeclaration.md)

- [Tips](/docs/UsageAndTips.md)

### Advanced Topics

- [Multithreading](/docs/Multithreading.md)

- [Multi Schema Support](/docs/MultiSchemaManagement.md)

- [Multi Database Support](/docs/MultiDatabaseManagement.md)

- [Change Metadata (and schema) on Runtime](/docs/RuntimeChanges.md)


---

## Roadmap

See [Roadmap](/docs/Roadmap.md) for planned features and improvements.


---

## Licensing and Contribution

Rapidex.Data is licensed under **LGPL v3**. See [License](LICENSE) for details.  

See: [Contributing](/docs/Contributing.md) for contribution guidelines.

## Thanks 

Thanks to all supporters!

Thanks to [SQL Kata](https://sqlkata.com/)