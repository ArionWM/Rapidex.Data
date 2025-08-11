using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Rapidex.Data.Configuration;

[Obsolete("//TODO: To Metadata.Data ...")]
internal class PredefinedValueProcessor : IPredefinedValueProcessor
{


    Dictionary<IDbEntityMetadata, PredefinedValueItem> _repository = new Dictionary<IDbEntityMetadata, PredefinedValueItem>();


    protected async Task Apply(IDbSchemaScope scope, IDbEntityMetadata em, IEnumerable<IEntity> entities, bool? @override)
    {
        if (em.OnlyBaseSchema && scope.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME)
            return;

        //scope.Structure.ApplyEntityStructure(em, false);

        var availEntities = await scope.GetQuery(em).Load();

        entities.ForEach(e =>
        {
            e._IsNew = true;
        });

        if (availEntities.ItemCount == 0)
        {
            entities.Save();
        }
        else
        {
            IDictionary<long, IEntity> dict = availEntities.ToIdDict();

            foreach (var entity in entities)
            {
                var avail = dict.Get(entity.GetId().As<long>());
                //availEntities.FirstOrDefault(e => object.Equals(e.GetId(), entity.GetId()));
                if (avail == null)
                {
                    entity.Save();
                }
                else
                {
                    if (@override.HasValue)
                    {
                        scope.Mapper.Copy(entity, avail);
                        avail.Save();
                    }
                }
            }
        }
        await scope.CommitOrApplyChanges();
    }

    protected async Task Apply(IDbSchemaScope scope, IDbEntityMetadata em)
    {
        PredefinedValueItem item = _repository.Get(em);
        if (item == null)
            return;

        if (em.OnlyBaseSchema && scope.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME)
            return;

        var newEntities = scope.Mapper.Map(em, item.Entities.Values); //.Clone(scope);

        await this.Apply(scope, em, newEntities, item.Override);
    }

    public async Task Apply(IDbSchemaScope scope)
    {
        foreach (var em in _repository.Keys)
        {
            await Apply(scope, em);
        }
    }

    public void Register(IDbEntityMetadata em, bool @override, params ObjDictionary[] entities)
    {
        PredefinedValueItem item = _repository.Get(em);
        if (item == null)
        {
            item = new PredefinedValueItem(em, @override);
            _repository.Add(em, item);
        }

        item.Override = @override;

        foreach (var entity in entities)
        {
            item.Entities.Set(entity["Id"].As<long>(), entity);
        }
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
        this._repository.Clear();
    }

    public void Clear(IDbEntityMetadata em)
    {
        throw new NotImplementedException();
    }

    public void Remove(IDbEntityMetadata em, long id)
    {
        throw new NotImplementedException();
    }

    public PredefinedValueItem Get(IDbEntityMetadata em)
    {
        return this._repository.Get(em);
    }

    public async Task Apply(IDbSchemaScope scope, IEnumerable<IEntity> unregisteredData, bool @override)
    {
        var group = unregisteredData.GroupBy(ent => ent.GetType().Name);
        foreach (var item in group)
        {
            var em = Database.Metadata.Get(item.Key);
            if (em == null)
                continue; //??

            IEnumerable<IEntity> entities = item;
            await this.Apply(scope, em, entities, @override);
        }
    }
}
