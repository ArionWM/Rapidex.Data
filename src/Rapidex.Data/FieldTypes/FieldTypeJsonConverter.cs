using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.FieldTypes;

//internal class FieldTypeJsonConverter<T> : JsonConverter<T> where T : IDataType
//{

//    public override bool CanConvert(Type typeToConvert)
//    {
//        return typeToConvert.IsSupportTo<IDataType>();
//    }

//    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        IDataType dt = TypeHelper.CreateInstance<IDataType>(typeToConvert);
//        object objValue = reader.GetValueAsOriginalType(() => { return typeof(ObjDictionary); });
//        reader.Skip();

//        dt.SetWithSerializationData(null, objValue);
//        return (T)dt;
//    }

//    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
//    {
//        object objSdata = value.GetSerializationData(EntitySerializationOptions.Default);
//        JsonSerializer.Serialize(writer, objSdata);
//        //JsonConverter converter = JsonSerializerOptions.Default.GetConverter(objSdata.GetType());
//        ////converter.Write(writer, objSdata, options);
//    }


//}

//internal class FieldTypeJsonConverterBDT : FieldTypeJsonConverter<BasicBaseDataType>
//{
//    public static void Register()
//    {
//        FieldTypeJsonConverterBDT conv = new FieldTypeJsonConverterBDT();
//        JsonHelper.Register(conv);
//    }
//}
