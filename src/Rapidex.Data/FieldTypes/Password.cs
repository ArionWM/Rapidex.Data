using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class Password : BasicBaseDataType<string>
    {
        private const string CRIPTO_TEXT_PREFIX = "|C|";
        private const string CRIPTO_TEXT_DUMMY = "*****";

        public override string TypeName => "password";



        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            self.DbProperties.Length = 200;
            return base.SetupMetadata(container, self, values);
        }



        public override object Clone()
        {
            Password clone = new Password();
            clone.Value = this.Value;
            return clone;
        }

        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        protected string CheckCryptoText(IEntity parent, string value)
        {
            if (value.IsNullOrEmpty() || value.StartsWith(CRIPTO_TEXT_PREFIX))
                return value;

            long id = (long)parent.GetId();
            if (id < 0)
                throw new InvalidOperationException("Cant do for premature entities");

            string key = parent._Schema.SchemaName.ToFriendly() + id.ToString(); //Tenant name'i de eklemek?
            string cryptoText = CRIPTO_TEXT_PREFIX + CryptoHelper.EncryptAes(value, key);
            return cryptoText;
        }

        protected void CheckCryptoText()
        {
            if (this.Value.IsNullOrEmpty())
                return;

            IEntity ent = this.GetParent().NotNull("Parent entity can't be null");
            long id = (long)ent.GetId();
            if (id < 0)
                return;

            string value = this.Value;
            value = this.CheckCryptoText(ent, value);
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
            if (value is Password psw)
                value = psw.Value;

            string _value = value.As<string>();
            if (_value.IsNOTNullOrEmpty() && _value.StartsWith('*'))
                return; //Dummy veri bize geri dönmüş. İşlemiyoruz.

            if (!entity._IsNew && entity.GetId().As<long>() > -1)
                _value = this.CheckCryptoText(entity, _value);

            base.SetValue(entity, fieldName, _value, applyToEntity);
        }

        public override object GetValueLower()
        {
            this.CheckCryptoText();
            return base.GetValueLower();
        }

        public override object GetValueUpper(IEntity entity, string fieldName)
        {
            Password passw = (Password)base.GetValueUpper(entity, fieldName);
            passw.CheckCryptoText();
            return passw;
        }


        public string Decrypt()
        {
            IEntity ent = this.GetParent().NotNull("Parent entity can't be null");
            long id = (long)ent.GetId();
            if (id < 0)
                return this.Value;

            string _value = this.Value;
            if (_value.IsNullOrEmpty() || !_value.StartsWith(CRIPTO_TEXT_PREFIX))
                return this.Value;

            _value = _value.Remove(0, CRIPTO_TEXT_PREFIX.Length);

            string key = ent._Schema.SchemaName.ToFriendly() + id.ToString(); //Tenant name'i de eklemek?
            string value = CryptoHelper.DecryptAes(_value, key);
            return value;
        }

        public static implicit operator Password(string value)
        {
            return new Password() { Value = value };
        }

        public static implicit operator string(Password pass)
        {
            return pass?.Value;
        }
    }
}
