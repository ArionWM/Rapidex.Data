using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class EMail : BasicBaseDataType<string>
    {
        public override string TypeName => "email";

        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            self.DbProperties.Length = 200; 
            return base.SetupMetadata(container, self, values);
        }

        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            EMail clone = new EMail();
            clone.Value = this.Value;
            return clone;
        }


        public static implicit operator EMail(string value)
        {
            return new EMail() { Value = value };
        }

        public static implicit operator string(EMail mail)
        {
            return mail?.Value;
        }

        
    }

}
