using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class Color : BasicBaseDataType<string>
    {
        public override string TypeName => "color";

        public override IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values)
        {
            self.DbProperties.Length = 20;
            return base.SetupMetadata(container, self, values);
        }

        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            Color clone = new Color();
            clone.Value = this.Value;
            return clone;
        }


        public static implicit operator Color(string value)
        {
            return new Color() { Value = value };
        }

        public static implicit operator string(Color color)
        {
            return color?.Value;
        }

        public override string ToString()
        {
            return this.Value;
        }

    }
}
