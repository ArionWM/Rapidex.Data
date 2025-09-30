using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class PasswordJsonConverter : JsonConverter<Password>
{
    public override Password? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            string value = reader.GetString();
            
            // Don't deserialize dummy password values
            if (value == OneWayPassword.CRIPTO_TEXT_DUMMY)
                return new Password();

            Password password = new Password();
            password.Value = value;
            return password;
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing Password.");
    }

    public override void Write(Utf8JsonWriter writer, Password value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(OneWayPassword.CRIPTO_TEXT_DUMMY);
    }


    public static void Register()
    {
        PasswordJsonConverter conv = new PasswordJsonConverter();
        JsonHelper.Register(conv);
    }
}
