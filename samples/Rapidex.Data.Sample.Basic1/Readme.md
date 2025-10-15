# Rapidex.Data.Sample.Basic1

This sample demontrates the basic (single database and schema) usage of Rapidex.Data library.

## Contents

- Concrete entity definition
- YAML entity definition
- Predefined data with YAML
- JSON based CRUD operations with ASP.NET Core Web API

## Usage

- Create your database (SQL Server or PostgreSQL) and update connection string in *appsettings.json*
- Create / or use existing database user with required rights. See: [Database Connection and Required Rights](../docs/DatabaseConnectionAndRequiredRights.md)
- Run the application. It will create required tables and predefined data automatically.
- For ready to use web API
	- Install [Postman](https://www.postman.com/downloads/)
	- Import [SampleRequestsPostmanCollection.json](SampleRequestsPostmanCollection.json) file.

