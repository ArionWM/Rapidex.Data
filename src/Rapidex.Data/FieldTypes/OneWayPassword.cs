using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data;


public class OneWayPassword : BasicBaseDataType<string>
{
    private const string CRIPTO_TEXT_PREFIX = "|S|";
    internal const string CRIPTO_TEXT_DUMMY = "*****";

    public override string TypeName => "oneWayPassword";



    public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
    {
        self.DbProperties.Length = 200;
        return base.SetupMetadata(container, self, values);
    }



    public override object Clone()
    {
        OneWayPassword clone = new OneWayPassword();
        clone.Value = this.Value;
        return clone;
    }

    public override IValidationResult Validate()
    {
        throw new NotImplementedException();
    }

    protected string CheckCryptoText(string value)
    {
        if (value.IsNullOrEmpty() || value.StartsWith(CRIPTO_TEXT_PREFIX))
            return value;

        string cryptoText = CRIPTO_TEXT_PREFIX + CryptoHelper.HashPassword(value);
        return cryptoText;
    }

    protected void CheckCryptoText()
    {
        if (this.Value.IsNullOrEmpty())
            return;

        string value = this.Value;
        value = this.CheckCryptoText(value);
        this.Value = value;
    }

    public override void SetValuePremature(object value)
    {
        string _value = value.As<string>();
        if (_value.IsNOTNullOrEmpty() && _value.StartsWith('*'))
            return; //Dummy veri bize geri dönmüş. İşlemiyoruz.

        base.SetValuePremature(value);
    }

    public override IPartialEntity[] SetValue(IEntity entity, string fieldName, ObjDictionary value)
    {
        return base.SetValue(entity, fieldName, value);
    }

    public override void SetValue(IEntity entity, string fieldName, object value, bool applyToEntity)
    {
        if (value is OneWayPassword owp)
            value = owp.Value;

        string _value = value.As<string>();
        if (_value.IsNOTNullOrEmpty() && _value.StartsWith('*'))
            return; //Dummy veri bize geri dönmüş. İşlemiyoruz.


        _value = this.CheckCryptoText(_value);
        base.SetValue(entity, fieldName, value, applyToEntity);
    }

    public override object GetValueLower()
    {
        this.CheckCryptoText();
        return base.GetValueLower();
    }

    public override object GetValueUpper(IEntity entity, string fieldName)
    {
        OneWayPassword passw = (OneWayPassword)base.GetValueUpper(entity, fieldName);
        passw.CheckCryptoText();
        return passw;
    }

    //public override object GetSerializationData(EntitySerializationOptions options)
    //{
    //    //Serileştirme verisine içeriği vermiyoruz.
    //    return CRIPTO_TEXT_DUMMY;
    //}

    public bool IsEqual(string comparedValue)
    {
        if (this.Value.IsNullOrEmpty())
            return false;

        string _val = this.Value.Remove(0, 3);
        return CryptoHelper.ValidatePassword(_val, comparedValue);

    }

    public static implicit operator OneWayPassword(string value)
    {
        return new OneWayPassword() { Value = value };
    }

    public static implicit operator string(OneWayPassword pass)
    {
        return pass?.Value;
    }
}
