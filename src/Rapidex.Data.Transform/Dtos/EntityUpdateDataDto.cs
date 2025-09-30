using Rapidex.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Rapidex.Data.Transform;

public enum EntityUpdateDtoType
{
    UpdateEntity,
    AddRelation,
    RemoveRelation,
}

[Obsolete("", true)]
public class EntityUpdateDataDto : EntityDataDto, IJsonOnDeserialized
{
    [JsonPropertyOrder(-9000)]
    public EntityUpdateDtoType UpdType { get; set; } = EntityUpdateDtoType.UpdateEntity;

    [JsonPropertyOrder(-9000)]
    public string EntityNameInternal { get; set; }

    [JsonPropertyOrder(-8000)]
    public bool IsNew { get; set; } = false;

    [JsonPropertyOrder(-8000)]
    public bool IsDeleted { get; set; } = false;

    //Note; We can use JsonExtensionData there. And support '$.Values.*' and '$.*' for serialization. Eg. in swagger. 
    [JsonPropertyOrder(7000)]
    [JsonExtensionData]
    public override ObjDictionary Values { get; set; } = new ObjDictionary();


    public override void OnDeserialized()
    {
        JsonHelper.MsDeserializationCorrection(this.Values);
    }
}
