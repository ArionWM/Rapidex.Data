using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Conversion.Converters;

internal class StringToByteArrayConverter : ConverterBase<string, byte[]>
{
    public override object Convert(object from, Type toType)
    {
        string value = (string)from;
        byte[] bytes;
        bytes = System.Convert.FromBase64String(value);
        return bytes;
    }

    public override bool TryConvert(object from, Type toType, out object to)
    {
        string value = (string)from;
        var minLength = ((value.Length * 3) + 3) / 4;
        Span<byte> buffer = new Span<byte>(new byte[minLength]);
        if (System.Convert.TryFromBase64String(value, buffer, out int bytesWritten))
        {
            to = buffer.Slice(0, bytesWritten).ToArray();
            return true;
        }
        else
        {
            to = null;
            return false;
        }
    }
}
