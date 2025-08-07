using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rapidex.Data;

[JsonDerivedBase]
public class EntityDataDtoBase : IJsonOnDeserialized
{
    public string Entity { get; set; }

    public object this[string key]
    {
        get { return this.Values.Get(key); }
        set { this.Values[key] = value; }
    }

    [JsonPropertyOrder(7000)]
    //Dikkat; JsonExtensionData kullanmıyoruz !!
    public virtual ObjDictionary Values { get; set; } = new ObjDictionary();

    [JsonIgnore]
    public IEnumerable<string> Keys => this.Values.Keys;

    public virtual void OnDeserialized()
    {
        //JsonHelper.MsDeserializationCorrection(this.Values);
    }
}
