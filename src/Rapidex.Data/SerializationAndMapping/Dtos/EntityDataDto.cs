using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data.SerializationAndMapping.Dtos;

[JsonDerivedBase]
public class EntityDataDto : IJsonOnDeserialized
{
    [JsonPropertyOrder(-9000)]
    public string Entity { get; set; }

    public object this[string key]
    {
        get { return this.Values.Get(key); }
        set { this.Values[key] = value; }
    }

    //Note; We not use JsonExtensionData there. Only use '$.Values.*' for serialization. Eg. in swagger.  
    [JsonPropertyOrder(7000)]
    public virtual ObjDictionary Values { get; set; } = new ObjDictionary();

    [JsonIgnore]
    public IEnumerable<string> Keys => this.Values.Keys;

    public virtual void OnDeserialized()
    {
        //JsonHelper.MsDeserializationCorrection(this.Values);
    }
}
