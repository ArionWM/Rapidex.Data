namespace Rapidex.Data.Sample.App1.ConcreteEntitites;

public class Country : DbConcreteEntityBase
{
    public string Name { get; set; }
    public string Code { get; set; } // ISO 3166-1 alpha-2 code
    public string Iso2 { get; set; }
    public string Iso3 { get; set; }
    public string CurrencySymbol { get; set; }
    public string PhoneCode { get; set; }
}

