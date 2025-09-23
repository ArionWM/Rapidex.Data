using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Rapidex.Data.Configuration;

[Obsolete("//TODO: To Metadata.Data ...", true)]
internal class PredefinedValueProcessor : IPredefinedValueProcessor
{


    Dictionary<IDbEntityMetadata, PredefinedValueItems> repository = new Dictionary<IDbEntityMetadata, PredefinedValueItems>();


    protected async Task Apply(IDbSchemaScope scope, IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        throw new NotSupportedException("obsolete");
    }

    protected async Task Apply(IDbSchemaScope scope, IDbEntityMetadata em)
    {
        throw new NotSupportedException("obsolete");
        //PredefinedValueItems item = _repository.Get(em);
        //if (item == null)
        //    return;

        //if (em.OnlyBaseSchema && scope.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME)
        //    return;

        //var newEntities = scope.Mapper.Map(em, item.Entities.Values); //.Clone(scope);

        //await this.Apply(scope, em, newEntities, item.Override);
    }

    public async Task Apply(IDbSchemaScope scope)
    {
        foreach (var em in this.repository.Keys)
        {
            await Apply(scope, em);
        }
    }

    public void Register(IDbEntityMetadata em, bool @override, params ObjDictionary[] entities)
    {
        throw new NotSupportedException("obsolete");
    }


    public void Register(IDbEntityMetadata em, JsonNode jdoc)
    {
        JsonNode predefinedNode = jdoc["data"];

        if (predefinedNode == null)
            return;

        if (predefinedNode["values"] == null)
            return;

        bool forceAllValues = predefinedNode["forceAllValues"]?.GetValue<bool>() ?? false;

        List<ObjDictionary> data = new List<ObjDictionary>();
        foreach (var item in predefinedNode["values"].AsArray())
        {
            ObjDictionary entity = new ObjDictionary();
            foreach (var field in item.AsObject().AsEnumerable())
            {
                IDbFieldMetadata fm = em.Fields.Get(field.Key);

                entity.Add(field.Key, field.Value?.GetValueAsOriginalType());
            }
            data.Add(entity);
        }

        this.Register(em, forceAllValues, data.ToArray());
    }

    public void Clear()
    {
        this.repository.Clear();
    }

    public void Clear(IDbEntityMetadata em)
    {
        throw new NotImplementedException();
    }

    public void Remove(IDbEntityMetadata em, long id)
    {
        throw new NotImplementedException();
    }

    public PredefinedValueItems Get(IDbEntityMetadata em)
    {
        return this.repository.Get(em);
    }

    public async Task Apply(IDbSchemaScope scope, IEnumerable<IEntity> unregisteredData, bool @override)
    {
        throw new NotSupportedException("obsolete");
    }
}
