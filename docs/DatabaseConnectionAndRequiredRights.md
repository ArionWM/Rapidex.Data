# Database Connection and Required Rights

## Master Database

Databases has symbolic names for easier access.

Default (first) database symbolic name is *master*. For single dabase applications, this is the only database you need.

For multi-tenant applications, each tenant can have its own database. See: [Multi Tenant Applications](MultiTenantApplications.md)

> The term *Master* is symbolic and has no relation with the *Master* database in Sql Server

## Connection String

You need to provide a valid connection string to connect to your *master* database.

For default, `appsettings.json` file is used to get connection string.

```Json
{
"Rapidex": {
    "Databases": {
        "Master": {
          "Provider": "Rapidex.Data.SqlServer.DbSqlServerProvider",
          "ConnectionString": "<connection string>"
        }
      }
  }
}
```

## Available Database Providers

Currently following database providers are available:

| Provider Name| Description|NuGet Package|Single DB Required User Rights|Multi DB Required User Rights|
|----|----|-----|-----|-----|
| Rapidex.Data.SqlServer.DbSqlServerProvider | Microsoft SQL Server| Rapidex.Data.SqlServer|DbOwner (for *master* db)|DbCreator|
| Rapidex.Data.PostgreServer.PostgreSqlServerProvider | PostgreSQL|Rapidex.Data.PostgreServer|should be database owner|CreateDb|


## Required Rights

For single database use, the user specified in the connection string must have DbOwner rights.

For multi database use (multi tenant applications), the user specified in the connection string must have CreateDb / DbCreator rights. The Rapidex.Data will create and manage tenant databases.