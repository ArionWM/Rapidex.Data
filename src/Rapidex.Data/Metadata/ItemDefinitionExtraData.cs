using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Rapidex.Data;

//TODO: Rename or refactor
public class ItemDefinitionExtraData : IJsonOnDeserialized
{
    public string Type { get; set; }

    public string Name { get; set; }
    public string Icon { get; set; }
    public string Target { get; set; }
    

    public object this[string key] { get { return this.Data.Get(key); } set { this.Data.Set(key, value); } }

    //[JsonConverter(typeof(JsonExtensionConverter))]
    [JsonExtensionData]
    public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

    public List<object> Options { get; set; } = new List<object>();

    public void AddData(string key, object value)
    {
        this.Data[key] = value;
    }

    public void AddData(StringDictionary dict)
    {
        foreach (var item in dict)
        {
            this.Data[item.Key] = item.Value;
        }
    }

    public void OnDeserialized()
    {
        JsonHelper.MsDeserializationCorrection(this.Data);
    }
}
