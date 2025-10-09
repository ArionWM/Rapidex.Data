# Predefined Data and Demo Data

Much applications need predefined data for some entities (like Country, Currency, etc.). *Rapidex.Data* provides an easy way to define and apply this data with YAML format.

Different predefined data sets can be created for different databases (tenants).

## Sample

Define entity (concrete or soft) and add predefined data for it.

```csharp
public class Country : DbConcreteEntityBase
{
    public string Name { get; set; }
    public string Code { get; set; } // ISO 3166-1 alpha-2 code
    public string Iso2 { get; set; }
    public string Iso3 { get; set; }
    public string CurrencySymbol { get; set; }
    public string PhoneCode { get; set; }
}
```

Create yaml file and place it in the any folder you use for entity definitions

```yaml
_tag: data 
forceAllValues: true
data:
  - entity: country
    id: 36
    name: Australia
    code: AU
    iso2: AU
    iso3: AUS
    phoneCode: 61
    currencySymbol: "AUD" # 36

  - entity: country
    id: 840
    name: United States
    code: US
    iso2: US
    iso3: USA
    phoneCode: 1
    currencySymbol: "USD" # 840
```

```csharp
var db = Database.Dbs.AddMainDbIfNotExists();
//...
db.Metadata.ScanDefinitions(@".\App_Content\MyAppDefinitions"); //<-- Load definitions include predefined data
//...
db.Structure.ApplyAllStructure(); //<-- Apply structure changes and predefined data
```