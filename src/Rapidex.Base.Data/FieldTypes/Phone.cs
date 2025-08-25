using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class Phone : BasicBaseDataType<string, Phone>
    {
        public override string TypeName => "phone";

        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            self.DbProperties.Length = 50; 
            return base.SetupMetadata(container, self, values);
        }

        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            Phone clone = new Phone();
            clone.Value = this.Value;
            return clone;
        }


        public static implicit operator Phone(string value)
        {
            return new Phone() { Value = value };
        }

        public static implicit operator string(Phone value)
        {
            return value.Value;
        }
    }

}
