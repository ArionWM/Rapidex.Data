using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Helpers;
public class PredefinedDataCollection
{
    public Dictionary<IDbEntityMetadata, PredefinedValueItems> Repository { get; } = new();


    public IDbMetadataContainer Parent { get; }

    public PredefinedDataCollection(IDbMetadataContainer parent)
    {
        this.Parent = parent;
    }


    public void Add(IDbEntityMetadata em, bool @override, params ObjDictionary[] entities)
    {
        PredefinedValueItems item = this.Repository.Get(em);
        if (item == null)
        {
            item = new PredefinedValueItems(em, @override);
            this.Repository.Add(em, item);
        }

        item.Override = @override;

        foreach (var entity in entities)
        {
            item.Entities.Set(entity["Id"].As<long>(), entity);
        }
    }

    public bool Contains(IEntity entity)
    {
        if (entity == null)
            return false;

        IDbEntityMetadata em = entity._Metadata.NotNull();

        PredefinedValueItems item = this.Repository.Get(em);
        if (item == null)
            return false;

        return item.Entities.ContainsKey(entity.GetId().As<long>());
    }


    public void Clear()
    {
        this.Repository.Clear();
    }

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
        PredefinedValueItems item = this.Repository.Get(em);
        if (item == null)
            return;

        if (em.OnlyBaseSchema && scope.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME)
            return;

        var newEntities = scope.Mapper.Map(em, item.Entities.Values); //.Clone(scope);

        await this.Apply(scope, em, newEntities, item.Override);
    }

    public async Task Apply(IDbSchemaScope targetSchema, bool? @override = null)
    {
        foreach (var em in Repository.Keys)
        {
            await this.Apply(targetSchema, em);
        }
    }
}
