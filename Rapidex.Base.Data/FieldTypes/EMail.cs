using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class EMail : BasicBaseDataType<string, EMail>
    {
        public override string TypeName => "email";

        public override IDbFieldMetadata SetupMetadata(IDbEntityMetadataManager containerManager, IDbFieldMetadata self, ObjDictionary values)
        {
            self.DbProperties.Length = 200; 
            return base.SetupMetadata(containerManager, self, values);
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
