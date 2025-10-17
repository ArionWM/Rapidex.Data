using Rapidex.Data.Sample.Library.ConcreteEntities;

namespace Rapidex.Data.Sample.Library.ConcreteEntities;

public class Country : DbConcreteEntityBase
{
    public string Name { get; set; }
    public string Code { get; set; } // ISO 3166-1 alpha-2 code
    public string Iso2 { get; set; }
    public string Iso3 { get; set; }
    public string CurrencySymbol { get; set; }
    public string PhoneCode { get; set; }

    public RelationOne2N<State> States { get; set; }
    public RelationOne2N<City> Cities { get; set; }
}

public class CountryImplementer : IConcreteEntityImplementer<Country>
{

    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        var countryEm = metadata;
        countryEm.MarkOnlyBaseSchema();
    }
}

