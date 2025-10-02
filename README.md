# Rapidex.Data

## What is Rapidex.Data?

Rapidex.Data is an Object Relational Mapping (ORM) framework with a focus on *flexibility* and *ease of use*. 

It allows to entity definitions with concrete (classes), JSON, YAML files or with code in runtime. Also entity definitions (metadata) can be change and apply in runtime.

Rapidex.Data supports multiple database engines (MS SQL Server, PostgreSQL and others planned) and multiple database connections in a single application.

## Main Features

- Define entities with **concrete** (classes) or **soft** (JSON, YAML or with code in runtime) 
- Change metadata (entity definitions) and schema in runtime
- Multiple database support in same application (multi tenant)
- Multiple schema (workspaces) support
- Different metadata customization for each database connection (each tenant can have different configuration)
- UnitOfWork pattern
- Unique and extensible *behavior* approach for reusable logic
- Dynamic logic injection with *signal hub (with Rapidex Common)* 
- Direct JSON support for CRUD operations
- Predefined data and demo data support

See: [Features](/docs/Features-detailed.md) for more details.


## Concept

...

## Getting Started

...

## Quick Start

See: [Quick Start](/docs/QuickStart.md) 

## Usage Samples

- [Tips](/docs/UsageAndTips.md)


## Sample Projects

You can download and run sample projects from the following links:

- [Sample Console Application](/samples/Rapidex.Data.ConsoleApp)
- [Sample ASP.NET Core Application](/samples/Rapidex.Data.AspNetCoreApp)

## Documentation

### Basics

- [Field Types](/docs/FieldTypes.md)

- [Entity Definition](/docs/EntityDefinition.md)

- [Behaviors](/docs/Behaviors.md)

- [Predefined Data and Demo Data](/docs/PredefinedData.md)

- [Querying Data](/docs/QueryingData.md)
  - [Filtering](/docs/Filtering.md)

- [Updating Data](/docs/UpdatingData.md)

- [Library Declaration](/docs/LibraryDeclaration.md)

### Advanced Topics

- [Multithreading](/docs/Multithreading.md)

- [Multi Schema Support](/docs/MultiSchemaManagement.md)

- [Multi Database Support](/docs/MultiDatabaseManagement.md)

- [Change Metadata (and schema) on Runtime](/docs/RuntimeChanges.md)

- [Using Test Data Creators](/docs/TestDataCreators.md)

## Roadmap

See [Roadmap](/docs/Roadmap.md) for planned features and improvements.

## Packages (nuget)

| Package | Version |
|---|---|
|[Rapidex.Data](https://www.nuget.org/packages/abc/)|![JobMan](https://img.shields.io/nuget/v/abc)|
|[Rapidex.Data.SqlServer](https://www.nuget.org/packages/abc/)|![JobMan](https://img.shields.io/nuget/v/abc)|
|[Rapidex.Data.PostgreServer](https://www.nuget.org/packages/abc/)|![JobMan](https://img.shields.io/nuget/v/abc)|

## Licensing and Contribution

Rapidex.Data is licensed under **LGPL v3**. See [License](LICENSE) for details.  

Contributions require approval of a **Contributor License Agreement (CLA)**. See [CLA](/docs/license/CONTRIBUTOR_LICENSE_AGREEMENT.md) for details.

## Thanks 

Thanks to all the contributors and supporters!

Thanks to [SQL Kata](https://sqlkata.com/)