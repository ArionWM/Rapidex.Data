using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class Percent : BasicBaseDataType<int, Percent>
    {
        public override string TypeName => "percent";

        public override object Clone()
        {
            Percent clone = new Percent();
            clone.Value = this.Value;
            return clone;
        }

        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public override IPartialEntity[] SetValue(IEntity entity, string fieldName, ObjDictionary value)
        {
            return base.SetValue(entity, fieldName, value);
        }


        //Create implicit conversions from int to Percent and percent to int
        public static implicit operator Percent(int value)
        {
            return new Percent() { Value = (byte)value };
        }

        public static implicit operator int(Percent value)
        {
            return value.Value;
        }

    }
}
