using Rapidex.Data.Metadata.Columns;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Rapidex.Data;

[TypeConverter(typeof(Currency.CurrencyTypeConverter))]
public class Currency : BasicBaseDataType<decimal>
{
    public class CurrencyDbFieldMetadata : DbFieldMetadata
    {
        public CurrencyDbFieldMetadata()
        {

        }

        public CurrencyDbFieldMetadata(IDbFieldMetadata source)
        {
            this.Name = source.Name;
            this.Caption = source.Caption;
            this.Type = typeof(Currency);
            this.BaseType = typeof(decimal);
            this.DbType = DbFieldType.Decimal;
            this.DbProperties.Length = 20;
            this.DbProperties.Scale = 8;
            this.SkipDbVersioning = source.SkipDbVersioning;
        }

        public override void Setup(IDbEntityMetadata parentMetadata)
        {
            //Entity'e bir de Currency type sütunu ekliyoruz.
            base.Setup(parentMetadata);

            DbFieldMetadata typeCm = new DbFieldMetadata();
            typeCm.Name = Currency.GetTypeFieldName(this);
            typeCm.Caption = this.Caption;
            typeCm.Type = typeof(string);
            typeCm.BaseType = typeof(string);
            typeCm.DbType = DbFieldType.String;
            typeCm.SkipDbVersioning = this.SkipDbVersioning;
            typeCm.DbProperties.Length = 3;
            typeCm.Invisible = true;

            parentMetadata.AddFieldIfNotExist(typeCm);
        }
    }

    public class CurrencyTypeConverter : System.ComponentModel.TypeConverter
    {
        static Type[] supportedTypes = new Type[]
        {
            typeof(string),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long)
        };

        public override bool CanConvertFrom(ITypeDescriptorContext? context, [NotNull] Type sourceType)
        {
            if (supportedTypes.Contains(sourceType))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            switch (value)
            {
                case string s:
                    {
                        if (decimal.TryParse(s, out decimal d))
                            return new Currency { Value = d };
                        else
                            throw new Exception("Invalid string format for Currency");
                    }
                case decimal d:
                    return new Currency { Value = d };
                case double d:
                    return new Currency { Value = (decimal)d };
                case float f:
                    return new Currency { Value = (decimal)f };
                case int i:
                    return new Currency { Value = i };
                case long l:
                    return new Currency { Value = l };
                default:
                    return base.ConvertFrom(context, culture, value);
            }
        }
    }


    private string PrematureCurrencyType { get; set; }

    public string Type
    {
        get
        {
            return this.GetCurrencyType();
        }
        set
        {
            this.SetCurrencyType(value);
        }
    }

    public override string TypeName => "currency";


    public override void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity)
    {
        entity.NotNull();

        switch (value)
        {
            case Currency c:
                this.Value = c.Value;
                break;
            case decimal d:
                this.Value = d;
                break;
            case double d:
                this.Value = (decimal)d;
                break;
            case float f:
                this.Value = (decimal)f;
                break;
            case int i:
                this.Value = i;
                break;
            case long l:
                this.Value = l;
                break;
            case IDictionary<string, object> dict:
                this.Value = dict.Get("value", true).As<decimal>();
                this.Type = dict.Get("type").As<string>();
                break;
            default:
                if (value == null)
                    this.Value = 0;
                else
                    throw new Exception("Invalid value type for Currency field");
                break;
        }

        if (applyToEntity)
            entity.SetValue(fieldName, this);
    }

    protected string GetCurrencyType()
    {
        IDataType _this = this;
        if (_this.FieldMetadata == null)
            return this.PrematureCurrencyType;

        string typeFieldName = GetTypeFieldName(this);

        string typeValue = _this.Parent.NotNull().GetValue<string>(typeFieldName) ?? "USD"; //TODO: Şu anda default value verecek bir ortam yok, ileride değiştirilecek
        return typeValue;
    }

    protected void SetCurrencyType(string currencyCode)
    {
        IDataType _this = this;
        if (_this.FieldMetadata == null)
        {
            this.PrematureCurrencyType = currencyCode;
        }
        else
        {
            this.PrematureCurrencyType = null;
            string typeFieldName = GetTypeFieldName(this);
            _this.Parent.NotNull().SetValue(typeFieldName, currencyCode);
        }
    }

    public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
    {
        //BasicBaseDataTypeConverter converter = new BasicBaseDataTypeConverter();
        //Common.Converter.Register(converter);

        return new CurrencyDbFieldMetadata(self);
    }

    public override void SetupInstance(IEntity entity, IDbFieldMetadata fm)
    {
        base.SetupInstance(entity, fm);

        if (this.PrematureCurrencyType.IsNOTNullOrEmpty())
        {
            this.SetCurrencyType(this.PrematureCurrencyType);
            this.PrematureCurrencyType = null;
        }
    }

    public override IValidationResult Validate()
    {
        throw new NotImplementedException();
    }

    public override object Clone()
    {
        Currency clone = new Currency();
        clone.Value = this.Value;
        clone.Type = this.Type;
        return clone;
    }

    //public override object GetSerializationData(EntitySerializationOptions options)
    //{
    //    ObjDictionary data = new ObjDictionary();
    //    data["value"] = this.Value;
    //    data["type"] = this.Type;
    //    //symbolText
    //    //symbolHtml
    //    //symbolIcon
    //    return data;
    //}

    //public override object SetWithSerializationData(string memberName, object value)
    //{
    //    if (value is IDictionary<string, object> data)
    //    {
    //        this.Value = data["value"].As<decimal>(); //?.Get<decimal>("value", true)
    //        this.Type = data["type"].As<string>();
    //    }
    //    else
    //    {
    //        this.Value = value.As<decimal>();
    //    }


    //    return null;
    //}

    public override string ToString()
    {
        return this.Value.ToString();
    }

    //define implicit conversion from decimal to Currency
    public static implicit operator Currency(decimal value)
    {
        return new Currency { Value = value };
    }

    //define implicit conversion from Currency to decimal
    public static implicit operator decimal(Currency value)
    {
        return value.Value;
    }



    protected static string GetTypeFieldName(Currency currency)
    {
        IDataType _this = currency;
        return GetTypeFieldName(_this.FieldMetadata);
    }

    protected static string GetTypeFieldName(IDbFieldMetadata currencyFm)
    {
        currencyFm.NotNull();

        return currencyFm.Name + "Currency";
    }
}


