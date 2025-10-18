# Quick Start

## Install Rapidex.Data

To install Rapidex.Data from NuGet Package Manager Console in Visual Studio.

```bash
Install-Package Rapidex.Data
```

and the database provider package for your database engine, for example, for SQL Server:

```bash
Install-Package Rapidex.Data.SqlServer
```

## Configure Database Connection

First, you need to configure the database connection (`appsettings.json`)

```json
"Rapidex": {
    "Databases": {
        "Master": {
          "Provider": "Rapidex.Data.SqlServer.DbSqlServerProvider",
          "ConnectionString": "Data Source=databaseServer;Initial Catalog=databaseName;User Id=username;Password=userPassword;MultipleActiveResultSets=true;Pooling=true;Min Pool Size=10; Max Pool Size=500;TrustServerCertificate=True"
        }
     }
  }
```

or

```json
"Rapidex": {
    "Databases": {
        "Master": {
          "Provider": "Rapidex.Data.PostgreServer.PostgreSqlServerProvider",
          "ConnectionString": "User ID=username;Password=userPassword;Host=databaseServer;Port=5432;Database=databaseName;"
        }
     }
  }
```

## Create Entity Definitions

You can define your entities using concrete classes or JSON/YAML files. Here is an example of defining an entity using a concrete class:

```csharp
public class Country : DbConcreteEntityBase
{
    public string Name { get; set; }
    public string Code { get; set; } 
    public string Iso2 { get; set; }
    public string Iso3 { get; set; }
    public string CurrencySymbol { get; set; }
    public string PhoneCode { get; set; }
}

```

See: [Entity Definition](/docs/EntityDefinition.md) 

See: [Sample Application](/samples/)

## Load or Use UnitOfWork to Manage Data

```csharp
var db = Database.Dbs.Db();
var countries = db.GetQuery<Country>()
        .Like(nameof(Country.Name), "Uni%")
        .OrderBy(OrderDirection.Asc, nameof(Country.Name))
        .Load();

// or

var work = db.BeginWork();
var newRecord = work.New<Country>();
newRecord.Name = "New Country";
newRecord.Save();
work.CommitChanges();
```

See: [Querying Data](QueryingData.md)

See: [Updating Data](UpdatingData.md)

## Initialize Rapidex.Data and Start

Using Dependency Injection in your application, add Rapidex.Data services and start the infrastructure.

```csharp
//...

builder.Services.AddRapidexDataLevel(); //<- Add Rapidex services

//...

app.Services.StartRapidexDataLevel(); //<- Start Rapidex infrastructure

//...


var db = Database.Dbs.AddMainDbIfNotExists();
db.Metadata.AddIfNotExist<Country>(); //Add definition with manual or declare *Library* and auto load all definitions 
db.Structure.ApplyAllStructure();

app.Run();
```

See: [Library Declaration](LibraryDeclaration.md)

See: [Sample Application](/samples/)